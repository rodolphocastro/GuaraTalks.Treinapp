FROM python:3.9.6

ENV FLASK_APP network

WORKDIR /app

COPY requirements.txt requirements.txt
COPY network.py network.py 

RUN pip install -r requirements.txt

ENTRYPOINT [ "python", "network.py" ]