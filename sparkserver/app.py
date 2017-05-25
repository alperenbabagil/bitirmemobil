from flask import Blueprint
from ipython_genutils.py3compat import execfile

import Parameters
import enEski
import recomm_engine

main = Blueprint('main', __name__)

from tinydb import TinyDB, Query
import json
import requests
import re

import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

from flask import Flask, request


@main.route("/<int:user_id>/ratings/top/<int:count>", methods=["GET"])
def top_ratings(user_id, count):
    logger.debug("User %s TOP ratings requested", user_id)
    top_ratings = recomm_engine.get_top_ratings(user_id, count)
    return json.dumps(top_ratings)


@main.route("/<int:user_id>/ratings/<int:movie_id>", methods=["GET"])
def movie_ratings(user_id, movie_id):
    logger.debug("User %s rating requested for movie %s", user_id, movie_id)
    ratings = recomm_engine.get_ratings_for_movie_ids(user_id, [movie_id])
    return json.dumps(ratings)


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


from threading import Thread
import time
import _thread

RecThread = None


@main.route("/recommendation_request", methods=["POST"])
def recommendation_request():
    global RecThread
    user_data = json.loads(request.headers['userData'])
    dev_id = user_data["DeviceId"]

    auth_res = auth_user(user_data)
    if auth_res:
        reqJson = request.get_json(silent=True)
        movies = reqJson["pairs"]

        # animation films
        #movies = {'0298148': 5.0 ,'0126029': 4.0, '0317705': 5.0, '0351283': 4.0, '0389790': 5.0,'0358082': 5.0}
        # comedy
        #movies = {'0109830': 5.0, '1675434': 4.0, '0118799': 5.0, '0021749': 4.0, '0027977': 5.0,'0211915': 4.0,'1187043': 5.0,'0015864': 4.0,'0120382': 5.0,'0266543': 5.0}
        #'0358082': 5, '0110357': 5, '0382932': 5}


        movies = recomm_engine.get_mvl_ids_from_imdb_ids(movies)

        session_id = reqJson["sessionId"]
        # user_id_to_add=recomm_engine.get_last_user_id()
        user_id_to_add = 0
        tuples = []
        for k, v in movies:
            tuples.append((int(user_id_to_add), int(k), v))

        RecThread = Thread(target=startRecommendation, args=(user_id_to_add, tuples, dev_id, session_id))
        RecThread.start()
        # _thread.start_new_thread(startRecommendation, (user_id_to_add, tuples,dev_id))
        # thread1.start()
        # thread1.join()
        # res = startRecommendation(dev_id,movies)
        return "success"
        # if res:
        #     return "success"
        # else:
        #     return "fail"
    else:
        return "wrong_credentials"


def startRecommendation(user_id, tuples, dev_id, session_id):
    # region live

    # recomm_engine.add_ratings(tuples)
    # top_ratings = recomm_engine.get_top_ratings(int(user_id), 15)
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


    res = requests.post('https://onesignal.com/api/v1/notifications', json=dictToSend)
    print('response from server:', res.text)
    dictFromServer = res.json()
    ##RecThread.join()
    # return True

    # j = json.loads(user_str)
    # u = User(**j)
    # print(User.name)


@main.route("/getRecommendations", methods=["POST"])
def getRecommendations():
    iki = None
    try:
        # bir = request.form.keys()[0]
        iki = str(request.data)
        uc = request.data
    except Exception as e:
        print(e)
    data = {}
    data['Name'] = 'godfather'
    data['Genres'] = ['Drama', 'Action']
    data['ImdbImageUrl'] = None
    data['Rating'] = 3.4

    datas = [data, data]
    json_data = json.dumps(datas)
    return json_data


@main.route("/<int:user_id>/ratings", methods=["POST"])
def add_ratings(user_id):
    # get the ratings from the Flask POST request object
    try:
        # bir = request.form.keys()[0]
        iki = request.data
    except Exception as e:
        print(e)

    # ratings_list = request.form.keys()[0].strip().split("\n")
    ratings_list = str(request.data).split("\n")
    ratings_list = map(lambda x: x.split("::"), ratings_list)
    # create a list with the format required by the negine (user_id, movie_id, rating)
    ratings = map(lambda x: (user_id, int(x[0]), float(x[1])), ratings_list)
    # add them to the model using then engine API

    # recomm_engine.add_ratings(ratings)

    return json.dumps(ratings)


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


def create_app(spark_context, dataset_path):
    # recomm_engine = RecommendationEngine(spark_context, dataset_path)
    # recomm_engine = enEski()
    # asd=enEski.enEski(spark_context)
    # exec(open("C:\\Source\\bitirme\\enEski.py").read(), globals())

    # vasd=eski.RMSEEngine(None)
    recomm_engine.init(spark_context)

    # initiate db
    db = TinyDB('db.json')

    app = Flask(__name__)
    app.register_blueprint(main)
    return app

