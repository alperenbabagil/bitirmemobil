from flask import Blueprint

import Parameters
import recomm_engine

main = Blueprint('main', __name__)

from tinydb import TinyDB, Query
import json
import requests
import re

from threading import Thread


import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

from flask import Flask, request


@main.route("/getTop250Ids", methods=["POST"])
def getTop250Ids():
    top = get_top250()
    print(top)
    return json.dumps(top)


@main.route("/login", methods=["POST"])
def login():
    content = request.get_json(silent=True)
    print(content)
    return "true"


def auth_user(user_json):
    return True


@main.route("/sign_up", methods=["POST"])
def sign_up():
    user_dict = request.get_json(silent=True)
    db = TinyDB('db.json')
    Usr = Query()
    if db.get(Usr.Username == user_dict["Username"]) is None:
        db.insert(user_dict)
        return "true"
    else:
        return "user_exists"


RecThread = None


@main.route("/recommendation_request", methods=["POST"])
def recommendation_request():
    global RecThread

    session_id = 0  #########
    dev_id = '10acbfd9-20c2-4305-aff8-7290cab21972'  ###########
    auth_res = True  ###########

    try:
        user_data = json.loads(request.headers['userData'])
        auth_res = auth_user(user_data)
    except Exception as e:
        print(e)

    if auth_res:
        reqJson = request.get_json(silent=True)
        movies=None
        try:
            dev_id = user_data["DeviceId"]
            movies = reqJson["pairs"]
            session_id = reqJson["sessionId"]
        except Exception as e:
            print(e)


        if movies is None:
            #comedy
            movies = {
                '0109830': 5.0,
                '0027977': 5.0,
                '0211915': 5.0,
                '0088763': 5.0,
                '0449059': 5.0,
                '0012349': 5.0,
                '2278388': 5.0,
                '0053291': 5.0,
                '0032553': 5.0,
                '2562232': 5.0,
            }
        movies = recomm_engine.get_mvl_ids_from_imdb_ids(movies)




        # user_id_to_add=recomm_engine.get_last_user_id()
        user_id_to_add = 0
        tuples = []
        for k, v in movies:
            tuples.append((int(user_id_to_add), int(k), v))

        RecThread = Thread(target=startRecommendation, args=(user_id_to_add, tuples, dev_id, session_id))
        RecThread.start()
        return "success"
    else:
        return "wrong_credentials"

def startRecommendation(user_id, tuples, dev_id, session_id):
    # region live


    top_ratings = recomm_engine.getRecommendations(tuples, 15)
    imdb_ids = recomm_engine.get_imdb_ids_from_mvl_ids(top_ratings)
    dictToSend = {
        "app_id": Parameters.app_id,
        "include_player_ids": [dev_id],
        "data": {"movies": imdb_ids, "sessionId": session_id},
        "contents": {"en": "Your movie recommendations ready"}
    }

    # endregion

    # region test

    # tuples=[(int(user_id),1,4),(int(user_id),123,3),(int(user_id),32,5),(int(user_id),134,4)]
    # time.sleep(5)
    # dictToSend = {
    #     "app_id": Parameters.app_id,
    #     "include_player_ids": [dev_id],
    #     "data": {"movies": "[\"0050083\",\"0108052\",\"0110912\",\"0167260\"]","sessionId":session_id},
    #     "contents": {"en": "Your movie recommendations ready"}
    # }

    # endregion

    #sending notifications
    res = requests.post('https://onesignal.com/api/v1/notifications', json=dictToSend)
    print('response from server:', res.text)

def get_top250():
    top250_url = "http://akas.imdb.com/chart/top"
    r = requests.get(top250_url)
    html = r.text.split("\n")
    result = []
    for line in html:
        line = line.rstrip("\n")
        m = re.search(r'data-titleid="tt(\d+?)">', line)
        if m:
            _id = m.group(1)
            result.append(_id)
    #
    return result

def create_app(spark_context):

    recomm_engine.init(spark_context)
    # initiate db
    db = TinyDB('db.json')
    app = Flask(__name__)
    app.register_blueprint(main)
    return app

