import warnings

warnings.filterwarnings("ignore")
import torch
import transformers
from transformers import AutoTokenizer, AutoModelForSeq2SeqLM
from transformers import pipeline
import langchain
from langchain.llms import HuggingFacePipeline

# from langchain.llms import HuggingFacePipeline
import locale

locale.getpreferredencoding = lambda: "UTF-8"
import os
import re

from langchain.vectorstores import Chroma
from langchain.text_splitter import RecursiveCharacterTextSplitter

from langchain.chains import RetrievalQA
from langchain.document_loaders import TextLoader
from langchain.document_loaders import DirectoryLoader

from InstructorEmbedding import INSTRUCTOR
from langchain.embeddings import HuggingFaceInstructEmbeddings

from pydub import AudioSegment
import torchaudio
from transformers import WhisperProcessor, WhisperForConditionalGeneration

# Helper function to get cite sources (from which document or which pdf file?)

import textwrap

import time
from datetime import datetime
import select
import requests

# pattern = re.compile(r'\w+\([^)]+\)')


pattern = re.compile(r"[a-zA-Z_]+\([^)]*\)")
import logging

current_time = datetime.now().strftime("%d%m%Y_%H%M%S")
os.makedirs("log", exist_ok=True)
logging.basicConfig(
    filename=f"log/{current_time}.txt",
    level=logging.DEBUG,
    format="%(asctime)s - %(levelname)s - %(message)s",
)
print(f"Logs will be: log/{current_time}.txt")


def wrap_text_preserve_newlines(text, width=110):
    # Split the input text into lines based on newline characters
    lines = text.split("\n")

    # Wrap each line individually
    wrapped_lines = [textwrap.fill(line, width=width) for line in lines]

    # # Join the wrapped lines back together using newline characters
    # wrapped_text = '\n'.join(wrapped_lines)
    matches = re.findall(pattern, lines[-1])
    if len(matches) == 0:
        return "I do not understand."
    result = ";".join(matches)
    # print(matches)

    # result = match.group(0) if match else "I do not understand."
    return result


def process_llm_response(llm_response):
    print(f"[LLM Prediction]: {wrap_text_preserve_newlines(llm_response['result'])}")
    return wrap_text_preserve_newlines(llm_response["result"])


def init():
    tokenizer = AutoTokenizer.from_pretrained("google/flan-t5-xl")

    model = AutoModelForSeq2SeqLM.from_pretrained(
        "google/flan-t5-xl",
        load_in_4bit=True,
        device_map="cuda:0",
        torch_dtype="auto",
        # low_cpu_mem_usage=True,
    )
    pipe = pipeline(
        "text2text-generation",
        model=model,
        tokenizer=tokenizer,
        max_length=512,
    )

    local_llm = HuggingFacePipeline(pipeline=pipe)

    # # Transformer check if this works
    # print(local_llm('What is the capital of Korea?')) # Should be seoul

    # Load and process the text files
    loader = DirectoryLoader("./db1101/", glob="./*.txt", loader_cls=TextLoader)
    documents = loader.load()

    # splitting the text into
    from langchain.text_splitter import CharacterTextSplitter
    text_splitter = CharacterTextSplitter(
        separator = ".",
        chunk_size = 100,
        chunk_overlap  = 0,
        length_function = len,
        is_separator_regex = False,
    )
    texts = text_splitter.split_documents(documents)
    for text in texts:
        print(text)
    model_name = "hkunlp/instructor-xl"
    instructor_embeddings = HuggingFaceInstructEmbeddings(
        model_name=model_name, model_kwargs={"device": "cuda"}
    )

    # Init Chroma DB
    ###### This takes a long time ##########
    # Embed and store the texts
    # Supplying a persist_directory will store the embeddings on disk
    embedding = instructor_embeddings
    persist_directory = "1102Final"
    if os.path.exists(persist_directory):
        vectordb = Chroma(
            persist_directory=persist_directory, embedding_function=embedding
        )
    else:
        vectordb = Chroma.from_documents(
            documents=texts, embedding=embedding, persist_directory=persist_directory
        )
        vectordb.persist()
    # vectordb.persist()
    # Init Retreiver
    retriever = vectordb.as_retriever(search_kwargs={"k": 1})

    # Init Langchain
    # create the chain to answer questions
    qa_chain = RetrievalQA.from_chain_type(
        llm=local_llm, chain_type="stuff", retriever=retriever
    )

    return qa_chain


import socket




def process_client_connection(
    client_socket,
    client_address,
    processor,
    model,
    forced_decoder_ids,
    saved_audio_filename="received.wav",
):
    print(f"Accepted connection from {client_address}")
    while True:
        ready_to_read, _, _ = select.select([client_socket], [], [], 180)
        if not ready_to_read:
            print("Client closed the connection")
            break  # Break out of the client handling loop
        # Receive and print data from the client
        cnt = 0
        with open(saved_audio_filename, "wb") as file:
            while True:
                data = client_socket.recv(1024)
                if data.endswith(b"EOF_MARKER"):
                    file.write(data[: -len(b"EOF_MARKER")])
                    break
                file.write(data)
                cnt += 1
            file.close()
        """
        Step2: Transcribe wav file
        """
        waveform, sample_rate = torchaudio.load(saved_audio_filename)
        start_time = time.time()
        # Process the audio data and get the transcription
        input_features = processor(
            waveform.squeeze().numpy(),
            sampling_rate=sample_rate,
            return_tensors="pt",
        ).input_features
        predicted_ids = model.generate(
            input_features, forced_decoder_ids=forced_decoder_ids
        )
        transcription = processor.batch_decode(predicted_ids)
        result = transcription[0]
        result = result.replace(
            "<|startoftranscript|><|en|><|transcribe|><|notimestamps|>",
            "",
        )
        result = result.replace("<|endoftext|>", "")
        stt_elapsed_time = time.time() - start_time
        print(f"[STT result]->{result}, took {stt_elapsed_time} seconds")
        logging.info(f"stt:{result}, {stt_elapsed_time}")
        byte_data = result.encode("utf-8")
        try:
            client_socket.send(byte_data)
        except BrokenPipeError:
            print("Client closed the connection abruptly")
        """
        Step3: feed transcription to LLM
        """
        result = result.lstrip()
        result = result.rstrip()
        start = time.time()
        llm_response = qa_chain(f"what is the _special_command_ of {result}?")
        response = process_llm_response(llm_response)
        llm_elapsed_time = time.time() - start
        logging.info(f"llm:{response}, {llm_elapsed_time}")

        response = response.replace(" ", "").lower()
        if response == "language(spanish)":
            LANGUAGE = "spanish"
            print(f"Set to {LANGUAGE} mode")
        elif response == "language(english)":
            LANGUAGE = "english"
            print(f"Set to {LANGUAGE} mode")
        """
        Step4: Send the command to the headset
        """
        # print(f"response type: {type(response)}")

        byte_data = response.encode("utf-8")
        try:
            client_socket.send(byte_data)
        except BrokenPipeError:
            print("Client closed the connection abruptly")
        except Exception as e:
            print("????????????")
            print(e)

    print("Closing client socket")
    client_socket.close()


def init_server(qa_chain):
    os.makedirs("log", exist_ok=True)
    server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    host="127.0.0.1"
    port = 65431
    server_socket.bind((host, port))
    server_socket.listen(5)
    current_time = datetime.now().strftime("%d%m%Y_%H%M%S")
    print(f"Listening for connections on {host}:{port}")
    log_file_path = f"log/{current_time}.txt"
    # print(f"log file is located: {log_file_path}")
    LANGUAGE = "english"
    """
    Prepare STT model
    """
    # Load model and processor
    processor = WhisperProcessor.from_pretrained("openai/whisper-small")
    model = WhisperForConditionalGeneration.from_pretrained("./checkpoint-500")
    forced_decoder_ids = processor.get_decoder_prompt_ids(
        language="english", task="transcribe"
    )

    saved_audio_filename = "received.wav"
    try:
        while (
            True
        ):  # This loop ensures that the server continues to listen for new connections
            try:
                client_socket, client_address = server_socket.accept()
                process_client_connection(
                    client_socket, client_address, processor, model, forced_decoder_ids
                ),
            except Exception as client_e:
                print(f"Error occurred with client {client_address}: {client_e}")
    except Exception as e:
        print(f"Error occurred in server: {e}")
    finally:
        print("Shutting down server")
        server_socket.close()  # Ensure server socket is closed properly


if __name__ == "__main__":
    qa_chain = init()
    try:
        init_server(qa_chain)
    except KeyboardInterrupt:
        print("\nServer is shutting down. Goodbye!")
