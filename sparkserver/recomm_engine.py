# !/usr/bin/env python


import sys
import itertools
from math import sqrt
from operator import add
from os.path import join, isfile
import numpy as np

import sys, os
import Parameters

os.environ["PYSPARK_PYTHON"]="python3"

os.environ["SPARK_HOME"] = Parameters.spark_home
sys.path.append(Parameters.home_python)
sys.path.append(Parameters.home_py4j)

try:
    from pyspark import SparkContext
    from pyspark import SparkConf
    from pyspark.mllib.recommendation import ALS
    print("import success")
except ImportError as exx:
    print(exx)

sc=None
bestModel=None
movies=None
id_link_imdb_to_mvl=None
id_link=None
ratings_RDD=None
requested_movies_RDD=None
movies_titles_RDD=None
movies_rating_counts_RDD=None
bestRank = None
bestLambda = None
bestNumIter = None
training = None
numPartitions = None
test = None
numTest = None


def get_mvl_ids_from_imdb_ids(moviesDict):
    global id_link_imdb_to_mvl
    mvl_movies = []
    for key in moviesDict:
        if key in id_link_imdb_to_mvl:
            mvl_movies.append((id_link_imdb_to_mvl[key], moviesDict[key]))
    return mvl_movies

def get_imdb_ids_from_mvl_ids(result):
    ids = []
    for res in result:
        if res in id_link:
            ids.append(id_link[res])
    return ids


def getRecommendations(ratingTuples, recommNumber):
    global movies,sc,bestModel,ratings,numPartitions,training
    # ratingTuples eg: [(0,101,5.0),(0,102,5.0)]

    myRatingsRDD = sc.parallelize(ratingTuples, 1)
    training = ratings.filter(lambda x: x[0] < 6 and x[1][0]!=0) \
        .values() \
        .union(myRatingsRDD)\
        .repartition(numPartitions) \
        .cache()

    #training model according to user ratings with global best parameters
    bestModel = trainModel()

    bestModelRmse = computeRmse(bestModel, test, numTest)
    print("RMSE value according to the best model is: ")
    print(bestModelRmse)


    #preventing rated moves to show as recommendation
    myRatedMovieIds = set([x[1] for x in ratingTuples])
    candidates = sc.parallelize([m for m in movies if m not in myRatedMovieIds])

    #predicting movies
    predictions = bestModel.predictAll(candidates.map(lambda x: (0, x))).collect()

    #sort predictions according to predicted ratings
    recommendations = sorted(predictions, key=lambda x: x[2], reverse=True)[:recommNumber]

    #adding recommended movies to returning list
    reclist=[]
    loopnum=recommNumber
    ln=len(recommendations)
    if ln<recommNumber:
        loopnum=ln
    for i in range(loopnum):
        reclist.append(str(recommendations[i][1]))
    print("Movies recommended for you:")
    for i in range(len(recommendations)):
        print("%2d: %s" % (i + 1, movies[recommendations[i][1]]))
    return reclist

def closeEngine():
    global sc
    sc.close()

def parseRating(line):
    """
    Parses a rating record in MovieLens format userId::movieId::rating::timestamp .
    """
    fields = line.strip().split("::")
    ret= int(fields[3]) % 10, (int(fields[0]), int(fields[1]), float(fields[2]))
    return int(fields[3]) % 10, (int(fields[0]), int(fields[1]), float(fields[2]))

def parseMovie(line):
    """
    Parses a movie record in MovieLens format movieId::movieTitle .
    """
    fields = line.strip().split("::")
    return int(fields[0]), fields[1]


def computeRmse(model, data, n):
    """
    Compute RMSE (Root Mean Squared Error).
    """
    predictions = model.predictAll(data.map(lambda x: (x[0], x[1])))
    predictionsAndRatings = predictions.map(lambda x: ((x[0], x[1]), x[2])) \
        .join(data.map(lambda x: ((x[0], x[1]), x[2]))) \
        .values()
    return sqrt(predictionsAndRatings.map(lambda x: (x[0] - x[1]) ** 2).reduce(add) / float(n))

def trainModel():
    global bestRank, bestLambda, bestNumIter
    global training
    try:
        return ALS.train(training, bestRank, bestNumIter,bestLambda)
    except Exception as e:
        print(e)
    return None

def init(sparkContext):
    global sc,bestModel,movies,id_link_imdb_to_mvl,id_link,ratings_RDD
    global requested_movies_RDD,movies_titles_RDD,movies_rating_counts_RDD
    global bestRank,bestLambda,bestNumIter,numPartitions
    global training,ratings
    global myRatings
    global numTest, test

    sc = sparkContext



    # ratings is an RDD of (last digit of timestamp, (userId, movieId, rating))
    ratings = sc.textFile(Parameters.data_path + "ratings.csv").map(parseRating)

    # movies is an RDD of (movieId, movieTitle)
    movies= dict(sc.textFile(Parameters.data_path+'movies.csv').map(parseMovie).collect())


    numRatings = ratings.count()
    numUsers = ratings.values().map(lambda r: r[0]).distinct().count()
    numMovies = ratings.values().map(lambda r: r[1]).distinct().count()
    print("Got %d ratings from %d users on %d movies." % (numRatings, numUsers, numMovies))

    # split ratings into train (60%), validation (20%), and test (20%) based on the
    # last digit of the timestamp, add myRatings to train, and cache them
    # training, validation, test are all RDDs of (userId, movieId, rating)
    numPartitions = 4
    training = ratings.filter(lambda x: x[0] < 6) \
        .values() \
        .repartition(numPartitions) \
        .cache()

    validation = ratings.filter(lambda x: x[0] >= 6 and x[0] < 8) \
        .values() \
        .repartition(numPartitions) \
        .cache()

    test = ratings.filter(lambda x: x[0] >= 8).values().cache()

    numTraining = training.count()
    numValidation = validation.count()
    numTest = test.count()

    print("Training: %d, validation: %d, test: %d" % (numTraining, numValidation, numTest))

    # train models and evaluate them on the validation set

    ranks = Parameters.ranks_array
    lambdas = Parameters.lambdas_array
    numIters = Parameters.iteration_array

    bestValidationRmse = float("inf")
    bestRank = 0
    bestLambda = -1.0
    bestNumIter = -1


    #getting best model and model parameters
    for rank, lmbda, numIter in itertools.product(ranks, lambdas, numIters):
        # Build the recommendation model using ALS
        model=None
        try:
            model = ALS.train(training, rank, numIter, lmbda)
        except Exception as e:
            print(e)
        validationRmse = computeRmse(model, validation, numValidation)
        print("RMSE (validation) = %f for the model trained with " % validationRmse + \
              "rank = %d, lambda = %.1f, and numIter = %d." % (rank, lmbda, numIter))
        if (validationRmse < bestValidationRmse):
            bestModel = model
            bestValidationRmse = validationRmse
            bestRank = rank
            bestLambda = lmbda
            bestNumIter = numIter

    training = ratings.filter(lambda x: x[0] < 8) \
        .values() \
        .repartition(numPartitions) \
        .cache()



    testRmse = computeRmse(bestModel, test, numTest)

    # evaluate the best model on the test set
    print("The best model was trained with rank = %d and lambda = %.1f, " % (bestRank, bestLambda) \
          + "and numIter = %d, and its RMSE on the test set is %f." % (bestNumIter, testRmse))

    # compare the best model with a naive baseline that always returns the mean rating
    meanRating = training.union(validation).map(lambda x: x[2]).mean()
    baselineRmse = sqrt(test.map(lambda x: (meanRating - x[2]) ** 2).reduce(add) / numTest)
    improvement = (baselineRmse - testRmse) / baselineRmse * 100
    print("The best model improves the baseline by %.2f" % (improvement) + "%.")

    ratings_file_path = os.path.join(Parameters.data_path, 'ratings.csv')
    ratings_raw_RDD = sc.textFile(ratings_file_path)
    ratings_raw_data_header = ratings_raw_RDD.take(1)[0]

    ratings_RDD = ratings_raw_RDD.filter(lambda line: line != ratings_raw_data_header) \
        .map(lambda line: line.split("::")).map(
        lambda tokens: (int(tokens[0]), int(tokens[1]), float(tokens[2]))).cache()

    #to convert ids between movielens and imdb
    id_link = {}
    id_link_imdb_to_mvl = {}

    # to convert ids between movielens and imdb
    file = open(os.path.join(Parameters.data_path, 'links.csv'), 'r')
    for line in file:
        ids = line.split(",")
        id_link[ids[0]] = ids[1]
        id_link_imdb_to_mvl[ids[1]] = ids[0]

    # Load movies data for later use
    print("Loading Movies data...")
    movies_file_path = os.path.join(Parameters.data_path, 'movies.csv')
    movies_raw_RDD = sc.textFile(movies_file_path)
    movies_raw_data_header = movies_raw_RDD.take(1)[0]
    movies_RDD = movies_raw_RDD.filter(lambda line: line != movies_raw_data_header) \
        .map(lambda line: line.split("::")).map(lambda tokens: (int(tokens[0]), tokens[1], tokens[2])).cache()
    movies_titles_RDD = movies_RDD.map(lambda x: (int(x[0]), x[1])).cache()



# class RMSEEngine:
#     def getRecommendations(self, ratingTuples, recommNumber):
#         # ratingTuples eg: [(101,5.0),(102,5.0)]
#         myRatedMovieIds = set([x[0] for x in ratingTuples])
#         candidates = self.sc.parallelize([m for m in self.movies if m not in myRatedMovieIds])
#         predictions = self.bestModel.predictAll(candidates.map(lambda x: (0, x))).collect()
#         print(predictions[:10])
#         recommendations = sorted(predictions, key=lambda x: x[2], reverse=True)[:recommNumber]
#         print(predictions[:10])
#         print("Movies recommended for you:")
#         for i in range(len(recommendations)):
#             print("%2d: %s" % (i + 1, self.movies[recommendations[i][1]]))
#
#     def closeEngine(self):
#         self.sc.close()
#
#     def parseRating(self, line):
#         """
#         Parses a rating record in MovieLens format userId::movieId::rating::timestamp .
#         """
#         fields = line.strip().split("::")
#         return int(fields[3]) % 10, (int(fields[0]), int(fields[1]), float(fields[2]))
#
#     def parseMovie(self, line):
#         """
#         Parses a movie record in MovieLens format movieId::movieTitle .
#         """
#         fields = line.strip().split("::")
#         return int(fields[0]), fields[1]
#
#     def loadRatings(self, ratingsFile):
#         """
#         Load ratings from file.
#         """
#         if not isfile(ratingsFile):
#             print("File %s does not exist." % ratingsFile)
#             sys.exit(1)
#         f = open(ratingsFile, 'r')
#         ratings = filter(lambda r: r[2] > 0, [self.parseRating(line)[1] for line in f])
#         f.close()
#         if not ratings:
#             print("No ratings provided.")
#             sys.exit(1)
#         else:
#             return ratings
#
#     def computeRmse(self, model, data, n):
#         """
#         Compute RMSE (Root Mean Squared Error).
#         """
#         """ Returns a list of predicted ratings for input user and product pairs. Matrixteki ? olan yerleri dolduruyor."""
#         predictions = model.predictAll(data.map(lambda x: (x[0], x[1])))
#         predictionsAndRatings = predictions.map(lambda x: ((x[0], x[1]), x[2])) \
#             .join(data.map(lambda x: ((x[0], x[1]), x[2]))) \
#             .values()
#         return sqrt(predictionsAndRatings.map(lambda x: (x[0] - x[1]) ** 2).reduce(add) / float(n))
#
#     def __init__(self, sc):
#
#         # if __name__ == "__main__":
#         """if (len(sys.argv) != 2):
#             print("Usage: /path/to/spark/bin/spark-submit --driver-memory 2g " + \
#                   "MovieLensALS.py movieLensDataDir personalRatingsFile")
#             sys.exit(1)"""
#         # Mysql Connection
#         """Connection = pymysql.connect(host='localhost', user='root', password='1234',db='bitirme', charset='utf8mb4', cursorclass=pymysql.cursors.DictCursor)
#           cursor = Connection.cursor()"""
#         """query = ("SELECT name FROM movie_onlymovie")
#         cursor.execute(query)"""
#         """a = []
#         for name in cursor:
#              a.append(name)
#         a = np.array(a)"""
#         """list(cursor.fetchall())"""
#         """cursor.close
#         Connection.close()"""
#
#         # set up environment
#         # conf = SparkConf() \
#         #     .setAppName("MovieLensALS") \
#         #     .set("spark.executor.memory", "2g")
#         #
#         # self.sc = SparkContext(conf=conf)
#
#
#
#         # make personalized recommendations
