from flask import Flask, render_template
from db import get_db, close_db
import sqlalchemy
from logger import log

app = Flask(__name__)
app.teardown_appcontext(close_db)




import logging
from html import escape
from uuid import uuid4
import os

from telegram import InlineQueryResultArticle, InputTextMessageContent, Update
from telegram.constants import ParseMode,ChatAction
from telegram.ext import Application, CommandHandler, ContextTypes, InlineQueryHandler,CallbackContext
# import sqlite3 as lite

# import pandas as pd
# from openpyxl import load_workbook


# from google.oauth2 import service_account
# from googleapiclient.discovery import build
# from googleapiclient.http import MediaIoBaseDownload

# import io
# import fitz 

from PIL import Image
# Enable logging
logging.basicConfig(
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s", level=logging.INFO
)
# set higher logging level for httpx to avoid all GET and POST requests being logged
logging.getLogger("httpx").setLevel(logging.WARNING)

logger = logging.getLogger(__name__)

# database_path = os.path.dirname(__file__) + "\\MyData.db"







@app.route("/")
def index():
    return render_template("index.html")


@app.route("/health")
def health():
    log.info("Checking /health")
    db = get_db()
    health = "OK"
    try:
        # cur = con.cursor()    
        # cur.execute('SELECT SQLITE_VERSION()')
     
        # data = cur.fetchone()
     
        # health = "OK " + "SQLite version: %s" % data
        log.info(f"/health reported OK including database connection: {data}")
    except sqlalchemy.exc.OperationalError as e:
        msg = f"sqlalchemy.exc.OperationalError: {e}"
        log.error(msg)
    except Exception as e:
        msg = f"Error performing healthcheck: {e}"
        log.error(msg)

    return health
