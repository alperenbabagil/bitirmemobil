from __future__ import print_function
import os, sys

import math

import Parameters

# os.environ["SPARK_HOME"] = "F:\\bitirme\\spark-2.0.1-bin-hadoop2.7"
#
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python")
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python\\lib\\py4j-0.10.3-src.zip")
#


#os.environ['JAVA_HOME'] = Parameters.java_home

# os.environ["SPARK_HOME"] = "F:\\bitirme\\spark-2.0.1-bin-hadoop2.7"
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python")
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python\\lib\\py4j-0.10.3-src.zip")

# os.environ["SPARK_HOME"] = "C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7"
# sys.path.append("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7\\python")
# sys.path.append("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7\\python\\lib\\py4j-0.10.4-src.zip")
#



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

import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


def prt(T):
    print(T)
    return T


def get_counts_and_averages(ID_and_ratings_tuple):
    """Given a tuple (movieID, ratings_iterable) 
    returns (movieID, (ratings_count, ratings_avg))
    """
    nratings = len(ID_and_ratings_tuple[1])
    return ID_and_ratings_tuple[0], (nratings, float(sum(x for x in ID_and_ratings_tuple[1])) / nratings)


class RecommendationEngine:
    """A movie recommendation engine
    """

    def get_mvl_ids_from_imdb_ids(self, moviesDict):
        mvl_movies = []
        for key in moviesDict:
            if key in self.id_link_imdb_to_mvl:
                mvl_movies.append((self.id_link_imdb_to_mvl[key], moviesDict[key]))
        return mvl_movies

    def get_imdb_ids_from_mvl_ids(self, result):
        ids = []
        for res in result:
            if str(res[0]) in self.id_link:
                ids.append(self.id_link[str(res[0])])
        return ids

    def get_last_user_id(self):
        with open(Parameters.data_path + "ratings.csv", 'rb') as fh:
            first = next(fh).decode()

            fh.seek(-1024, 2)
            last = fh.readlines()[-1].decode()
            last = last.split("::")[0]
            return last

    def __count_and_average_ratings(self):
        """Updates the movies ratings counts from 
        the current data self.ratings_RDD
        """
        logger.info("Counting movie ratings...")
        movie_ID_with_ratings_RDD = self.ratings_RDD.map(lambda x: (x[1], x[2])).groupByKey()
        movie_ID_with_avg_ratings_RDD = movie_ID_with_ratings_RDD.map(get_counts_and_averages)
        self.movies_rating_counts_RDD = movie_ID_with_avg_ratings_RDD.map(lambda x: (x[0], x[1][0]))

    def __train_model(self):
        """Train the ALS model with the current dataset
        """
        logger.info("Training the ALS model...")
        self.model = ALS.train(self.ratings_RDD, self.rank, seed=self.seed,
                               iterations=self.iterations, lambda_=self.regularization_parameter)
        logger.info("ALS model built!")

    def __predict_ratings(self, user_and_movie_RDD):
        """Gets predictions for a given (userID, movieID) formatted RDD
        Returns: an RDD with format (movieTitle, movieRating, numRatings)
        """
        predicted_RDD = self.model.predictAll(user_and_movie_RDD)
        predicted_rating_RDD = predicted_RDD.map(lambda x: (x.product, x.rating))
        x = predicted_RDD.collect()
        y = predicted_RDD.takeOrdered(15, key=lambda x: -x[2])
        predicted_rating_title_and_count_RDD = \
            predicted_rating_RDD.join(self.movies_titles_RDD).join(self.movies_rating_counts_RDD)
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

    def add_ratings(self, ratings):
        """Add additional movie ratings in the format (user_id, movie_id, rating)
        """
        # Convert ratings to an RDD
        new_ratings_RDD = self.sc.parallelize(ratings)
        # Add new ratings to the existing ones
        self.ratings_RDD = self.ratings_RDD.union(new_ratings_RDD)
        # Re-compute movie ratings count
        self.__count_and_average_ratings()
        # Re-train the ALS model with the new ratings
        self.__train_model()

        return ratings

    def get_ratings_for_movie_ids(self, user_id, movie_ids):
        """Given a user_id and a list of movie_ids, predict ratings for them 
        """
        requested_movies_RDD = self.sc.parallelize(movie_ids).map(lambda x: (user_id, x))
        # Get predicted ratings
        ratings = self.__predict_ratings(requested_movies_RDD).collect()

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
                                                                                                           key=lambda
                                                                                                               x: -x[2])

        return ratings

    def __init__(self, sc, dataset_path):
        """Init the recommendation engine given a Spark context and a dataset path
        """
        # dataset_path = "F:\\bitirme\\bilal\\son\\MLlibSpark"
        logger.info("Starting up the Recommendation Engine: ")

        self.sc = sc

        # Load ratings data for later use
        logger.info("Loading Ratings data...")
        ratings_file_path = os.path.join(dataset_path, 'ratings.csv')
        ratings_raw_RDD = self.sc.textFile(ratings_file_path)
        ratings_raw_data_header = ratings_raw_RDD.take(1)[0]

        self.ratings_RDD = ratings_raw_RDD.filter(lambda line: line != ratings_raw_data_header) \
            .map(lambda line: line.split("::")).map(
            lambda tokens: (int(tokens[0]), int(tokens[1]), float(tokens[2]))).cache()

        self.id_link = {}
        self.id_link_imdb_to_mvl = {}

        file = open(os.path.join(dataset_path, 'links.csv'), 'r')
        for line in file:
            ids = line.split(",")
            self.id_link[ids[0]] = ids[1]
            self.id_link_imdb_to_mvl[ids[1]] = ids[0]

        # Load movies data for later use
        logger.info("Loading Movies data...")
        movies_file_path = os.path.join(dataset_path, 'movies.csv')
        movies_raw_RDD = self.sc.textFile(movies_file_path)
        movies_raw_data_header = movies_raw_RDD.take(1)[0]
        self.movies_RDD = movies_raw_RDD.filter(lambda line: line != movies_raw_data_header) \
            .map(lambda line: line.split("::")).map(lambda tokens: (int(tokens[0]), tokens[1], tokens[2])).cache()
        self.movies_titles_RDD = self.movies_RDD.map(lambda x: (int(x[0]), x[1])).cache()

        # for d in self.movies_titles_RDD.collect():
        #     print(d)

        # Pre-calculate movies ratings counts
        self.__count_and_average_ratings()

        # Train the model
        self.rank = 8
        self.seed = int(5)
        self.iterations = Parameters.iteration
        self.regularization_parameter = 0.1
        #self.__train_model()

        training_RDD, validation_RDD, test_RDD = self.ratings_RDD.randomSplit([6, 2, 2], seed=0)
        validation_for_predict_RDD = validation_RDD.map(lambda x: (x[0], x[1]))
        test_for_predict_RDD = test_RDD.map(lambda x: (x[0], x[1]))

        seed = 5
        iterations = 10
        regularization_parameter = 0.1
        ranks = [4, 8, 12]
        errors = [0, 0, 0]
        err = 0
        tolerance = 0.02

        min_error = float('inf')
        best_rank = -1
        best_iteration = -1
        for rank in ranks:

            model = ALS.train(training_RDD, rank, iterations=iterations,
                              lambda_=regularization_parameter)
            predictions = model.predictAll(validation_for_predict_RDD).map(lambda r: ((r[0], r[1]), r[2]))
            rates_and_preds = validation_RDD.map(lambda r: ((int(r[0]), int(r[1])), float(r[2]))).join(predictions)
            error = math.sqrt(rates_and_preds.map(lambda r: (r[1][0] - r[1][1]) ** 2).mean())
            errors[err] = error
            err += 1
            print('For rank %s the RMSE is %s' % (rank, error))
            if error < min_error:
                min_error = error
                best_rank = rank
                self.model = model
                self.rank=rank


        print('The best model was trained with rank %s' % best_rank)
