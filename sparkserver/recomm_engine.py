# !/usr/bin/env python

#TODO: class olmadığı için farklı thaeadlarda çalışmıyor ???
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

    print("success")

except ImportError as exx:
    print(exx)


    # TODO: Serverin açılışında best model oluşturulacak. rmse yazılacak. Sonraki recommendation requestler bu model kullanılarak yapılacak

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


def add_ratings(ratings):
    global sc,ratings_RDD
    """Add additional movie ratings in the format (user_id, movie_id, rating)
    """
    # Convert ratings to an RDD
    new_ratings_RDD = sc.parallelize(ratings)
    # Add new ratings to the existing ones
    ratings_RDD = ratings_RDD.union(new_ratings_RDD)
    # Re-compute movie ratings count
    __count_and_average_ratings()
    # Re-train the ALS model with the new ratings

    # Will be one time every day
    # self.__train_model()

    return ratings

def __predict_ratings(user_and_movie_RDD):
    global bestModel,movies_titles_RDD,movies_rating_counts_RDD
    """Gets predictions for a given (userID, movieID) formatted RDD
    Returns: an RDD with format (movieTitle, movieRating, numRatings)
    """
    predicted_RDD = bestModel.predictAll(user_and_movie_RDD)
    predicted_rating_RDD = predicted_RDD.map(lambda x: (x.product, x.rating))
    x = predicted_RDD.collect()
    y = predicted_RDD.takeOrdered(15, key=lambda x: -x[2])
    predicted_rating_title_and_count_RDD = \
        predicted_rating_RDD.join(movies_titles_RDD).join(movies_rating_counts_RDD)
    # predicted_rating_title_and_count_RDD = \
    #     predicted_rating_title_and_count_RDD.map(lambda r: (r[1][0][1], r[1][0][0], r[1][1]))

    # predicted_rating_title_and_count_RDD.map(lambda r:prt(r[1][0][1]))

    predicted_rating_title_and_count_RDD = \
        predicted_rating_title_and_count_RDD.map(lambda r: (r[0], str(r[1][0][1]), r[1][0][0], r[1][1]))

    vr = predicted_rating_title_and_count_RDD.collect()
    # predicted_rating_title_and_count_RDD = \
    #     predicted_rating_title_and_count_RDD.map(lambda r: (r[1][0][0], r[1][0][0], r[1][1]))
    #
    # predicted_rating_title_and_count_RDD = \
    #     predicted_rating_title_and_count_RDD.map(lambda r: (r[0][0][0], r[1][0][0], r[1][1]))

    return predicted_rating_title_and_count_RDD

def get_ratings_for_movie_ids(user_id, movie_ids):
    global sc,requested_movies_RDD
    """Given a user_id and a list of movie_ids, predict ratings for them
    """
    requested_movies_RDD = sc.parallelize(movie_ids).map(lambda x: (user_id, x))
    # Get predicted ratings
    ratings = __predict_ratings(requested_movies_RDD).collect()

    return ratings

def get_top_ratings(self, user_id, movies_count):
    """Recommends up to movies_count top unrated movies to user_id
    """
    # Get pairs of (userID, movieID) for user_id unrated movies
    user_unrated_movies_RDD = self.ratings_RDD.filter(lambda rating: not rating[0] == user_id) \
        .map(lambda x: (user_id, x[1])).distinct()

    x = user_unrated_movies_RDD.collect()
    # Get predicted ratings
    # ratings = self.__predict_ratings(user_unrated_movies_RDD).filter(lambda r: r[2]>=25).takeOrdered(movies_count, key=lambda x: -x[1])
    ratings = self.__predict_ratings(user_unrated_movies_RDD).filter(lambda r: r[3] >= 25).takeOrdered(movies_count,
                                                                                                       key=lambda x: -x[
                                                                                                           2])

    return ratings

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

def get_last_user_id(self):
    with open(Parameters.data_path + "ratings.csv", 'rb') as fh:
        first = next(fh).decode()

        fh.seek(-1024, 2)
        last = fh.readlines()[-1].decode()
        last = last.split("::")[0]
        return last

def __count_and_average_ratings():
    global ratings_RDD,movies_rating_counts_RDD
    """Updates the movies ratings counts from
    the current data self.ratings_RDD
    """
    print("Counting movie ratings...")
    movie_ID_with_ratings_RDD = ratings_RDD.map(lambda x: (x[1], x[2])).groupByKey()
    movie_ID_with_avg_ratings_RDD = movie_ID_with_ratings_RDD.map(get_counts_and_averages)
    movies_rating_counts_RDD = movie_ID_with_avg_ratings_RDD.map(lambda x: (x[0], x[1][0]))

def get_counts_and_averages(ID_and_ratings_tuple):
    """Given a tuple (movieID, ratings_iterable)
    returns (movieID, (ratings_count, ratings_avg))
    """
    nratings = len(ID_and_ratings_tuple[1])
    return ID_and_ratings_tuple[0], (nratings, float(sum(x for x in ID_and_ratings_tuple[1]))/nratings)

def getRecommendations(ratingTuples, recommNumber):
    global movies,sc,bestModel,ratings,numPartitions,training

    myRatingsRDD = sc.parallelize(ratingTuples, 1)

    # ratingTuples eg: [(101,5.0),(102,5.0)]

    #ratings=ratings.filter(lambda x: x[1][0] != 0)


    training = ratings.filter(lambda x: x[0] < 6 and x[1][0]!=0) \
        .values() \
        .union(myRatingsRDD)\
        .repartition(numPartitions) \
        .cache()

    # vr=training.collect()
    # son=vr[-1]
    # f=training.filter(lambda x:x[0]==0)
    # fl=f.collect()

    bestModel = trainModel()


    myRatedMovieIds = set([x[1] for x in ratingTuples])
    candidates = sc.parallelize([m for m in movies if m not in myRatedMovieIds])
    cands=candidates.collect()
    predictions = bestModel.predictAll(candidates.map(lambda x: (0, x))).collect()
    print(predictions[:10])
    recommendations = sorted(predictions, key=lambda x: x[2], reverse=True)[:recommNumber]
    reclist=[]
    loopnum=recommNumber
    ln=len(recommendations)
    if ln<recommNumber:
        loopnum=ln
    for i in range(loopnum):
        n=recommendations[i][1]
        reclist.append(str(recommendations[i][1]))
    print(predictions[:10])
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

def loadRatings(ratingsFile):
    """
    Load ratings from file.
    """
    if not isfile(ratingsFile):
        print("File %s does not exist." % ratingsFile)
        sys.exit(1)
    f = open(ratingsFile, 'r')
    ratings = filter(lambda r: r[2] > 0, [parseRating(line)[1] for line in f])
    f.close()
    if not ratings:
        print("No ratings provided.")
        sys.exit(1)
    else:
        return ratings

def computeRmse(model, data, n):
    """
    Compute RMSE (Root Mean Squared Error).
    """
    """ Returns a list of predicted ratings for input user and product pairs. Matrixteki ? olan yerleri dolduruyor."""
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


# sc=None
# bestModel=None
# movies=None
# id_link_imdb_to_mvl=None
# id_link=None
# ratings_RDD=None
# requested_movies_RDD=None
# movies_titles_RDD=None
# movies_rating_counts_RDD=None

def init(sparkContext):
    global sc,bestModel,movies,id_link_imdb_to_mvl,id_link,ratings_RDD
    global requested_movies_RDD,movies_titles_RDD,movies_rating_counts_RDD
    global bestRank,bestLambda,bestNumIter,numPartitions
    global training,ratings
    global myRatings

    sc = sparkContext

    # load personal ratings
    # myRatings = loadRatings(Parameters.data_path + "personalRatings.txt")
    # ms=list(myRatings)
    # myRatingsRDD = sc.parallelize(myRatings, 1)

    # load ratings and movie titles

    # ratings is an RDD of (last digit of timestamp, (userId, movieId, rating))
    ratings = sc.textFile(Parameters.data_path + "ratings.csv").map(parseRating)
    birli=ratings.filter(lambda x:x[1][0]==1)
    birlils=birli.collect()
    ls2=ratings.collect()


    # movies is an RDD of (movieId, movieTitle)

    try:
        tf = sc.textFile(Parameters.data_path+'movies.csv')
        mapped = tf.map(parseMovie)
        collected = mapped.collect()
        movies = dict(collected)
    except Exception as e:
        print(e)



    # file=sc.textFile("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\data\\movies.csv")
    # mapped=file.map(parseMovie)
    # try:
    #     collected=mapped.collect()
    # except Exception as e:
    #     print(e)
    # dicted=dict(collected)
    # movies = dict(sc.textFile(join("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\data", "movies.csv")).map(parseMovie).collect())
    # movies = dict(file.map(parseMovie).collect())

    numRatings = ratings.count()
    numUsers = ratings.values().map(lambda r: r[0]).distinct().count()
    numMovies = ratings.values().map(lambda r: r[1]).distinct().count()
    """45 saniye s¸rd¸"""
    print("Got %d ratings from %d users on %d movies." % (numRatings, numUsers, numMovies))

    # split ratings into train (60%), validation (20%), and test (20%) based on the
    # last digit of the timestamp, add myRatings to train, and cache them

    # training, validation, test are all RDDs of (userId, movieId, rating)

    # .union(myRatingsRDD) \
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
    """25saniye s¸rd¸"""
    print("Training: %d, validation: %d, test: %d" % (numTraining, numValidation, numTest))

    # train models and evaluate them on the validation set

    """Rank of the feature matrix. Ne kadar b¸y¸kse o kadar iyi fakat yava˛lat˝yor. Default - > 10 """
    ranks = Parameters.ranks_array
    """Regularization parameter.(default: 0.01)"""
    lambdas = Parameters.lambdas_array
    """Number of iterations of ALS. (default: 5) Yap˝lacaksa maximum 20 yap˝lmal˝ ."""
    numIters = Parameters.iteration_array

    bestValidationRmse = float("inf")
    bestRank = 0
    bestLambda = -1.0
    bestNumIter = -1



    for rank, lmbda, numIter in itertools.product(ranks, lambdas, numIters):
        # Build the recommendation model using Alternating Least Squares
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

    id_link = {}
    id_link_imdb_to_mvl = {}

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
