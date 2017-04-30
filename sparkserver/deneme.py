from flask import Blueprint

main = Blueprint('main', __name__)

import json
import requests
import re

from engine import RecommendationEngine

import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

from flask import Flask, request

vaar=None

def bir():
    print(vaar)

def iki():
    global vaar
    vaar=10

iki()
bir()


@main.route("/<int:user_id>/ratings/top/<int:count>", methods=["GET"])
def top_ratings(user_id, count):
    logger.debug("User %s TOP ratings requested", user_id)
    top_ratings = recommendation_engine.get_top_ratings(user_id, count)
    return json.dumps(top_ratings)


@main.route("/<int:user_id>/ratings/<int:movie_id>", methods=["GET"])
def movie_ratings(user_id, movie_id):
    logger.debug("User %s rating requested for movie %s", user_id, movie_id)
    ratings = recommendation_engine.get_ratings_for_movie_ids(user_id, [movie_id])
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
    return json.dumps(content)


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

    # recommendation_engine.add_ratings(ratings)

    return json.dumps(ratings)


def create_app(spark_context, dd):
    global recommendation_engine

    dataset_path = "F:\\bitirme\\bilal\\son\\MLlibSpark"
    # recommendation_engine = RecommendationEngine(spark_context, dataset_path)

    app = Flask(__name__)
    app.register_blueprint(main)
    return app


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

