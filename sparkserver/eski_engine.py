# !/usr/bin/env python

import sys
import itertools
from math import sqrt
from operator import add
from os.path import join, isfile
import numpy as np

import  sys, os
import Parameters

os.environ["PYSPARK_PYTHON"]="python3"


# os.environ["SPARK_HOME"] = "F:\\bitirme\\spark-2.0.1-bin-hadoop2.7"
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python")
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python\\lib\\py4j-0.10.3-src.zip")

# os.environ["SPARK_HOME"] = "C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7"
# sys.path.append("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7\\python")
# sys.path.append("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7\\python\\lib\\py4j-0.10.4-src.zip")
#
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

#TODO: Serverin açılışında best model oluşturulacak. rmse yazılacak. Sonraki recommendation requestler bu model kullanılarak yapılacak

class RMSEEngine:

    def getRecommendations(self,ratingTuples,recommNumber):
        #ratingTuples eg: [(101,5.0),(102,5.0)]
        myRatedMovieIds = set([x[0] for x in ratingTuples])
        candidates = self.sc.parallelize([m for m in self.movies if m not in myRatedMovieIds])
        predictions = self.bestModel.predictAll(candidates.map(lambda x: (0, x))).collect()
        print(predictions[:10])
        recommendations = sorted(predictions, key=lambda x: x[2], reverse=True)[:recommNumber]
        print(predictions[:10])
        print("Movies recommended for you:")
        for i in range(len(recommendations)):
            print("%2d: %s" % (i + 1, self.movies[recommendations[i][1]]))

    def closeEngine(self):
        self.sc.close()

    def parseRating(self,line):
        """
        Parses a rating record in MovieLens format userId::movieId::rating::timestamp .
        """
        fields = line.strip().split("::")
        return int(fields[3]) % 10, (int(fields[0]), int(fields[1]), float(fields[2]))


    def parseMovie(self,line):
        """
        Parses a movie record in MovieLens format movieId::movieTitle .
        """
        fields = line.strip().split("::")
        return int(fields[0]), fields[1]


    def loadRatings(self,ratingsFile):
        """
        Load ratings from file.
        """
        if not isfile(ratingsFile):
            print("File %s does not exist." % ratingsFile)
            sys.exit(1)
        f = open(ratingsFile, 'r')
        ratings = filter(lambda r: r[2] > 0, [self.parseRating(line)[1] for line in f])
        f.close()
        if not ratings:
            print("No ratings provided.")
            sys.exit(1)
        else:
            return ratings


    def computeRmse(self,model, data, n):
        """
        Compute RMSE (Root Mean Squared Error).
        """
        """ Returns a list of predicted ratings for input user and product pairs. Matrixteki ? olan yerleri dolduruyor."""
        predictions = model.predictAll(data.map(lambda x: (x[0], x[1])))
        predictionsAndRatings = predictions.map(lambda x: ((x[0], x[1]), x[2])) \
            .join(data.map(lambda x: ((x[0], x[1]), x[2]))) \
            .values()
        return sqrt(predictionsAndRatings.map(lambda x: (x[0] - x[1]) ** 2).reduce(add) / float(n))

    def __init__(self,sc):

    #if __name__ == "__main__":
        """if (len(sys.argv) != 2):
            print("Usage: /path/to/spark/bin/spark-submit --driver-memory 2g " + \
                  "MovieLensALS.py movieLensDataDir personalRatingsFile")
            sys.exit(1)"""
        # Mysql Connection
        """Connection = pymysql.connect(host='localhost', user='root', password='1234',db='bitirme', charset='utf8mb4', cursorclass=pymysql.cursors.DictCursor)
          cursor = Connection.cursor()"""
        """query = ("SELECT name FROM movie_onlymovie")
        cursor.execute(query)"""
        """a = []
        for name in cursor:
             a.append(name)
        a = np.array(a)"""
        """list(cursor.fetchall())"""
        """cursor.close
        Connection.close()"""

        # set up environment
        conf = SparkConf() \
            .setAppName("MovieLensALS") \
            .set("spark.executor.memory", "2g")

        self.sc = sc

        # load personal ratings
        myRatings = self.loadRatings(Parameters.data_path+"personalRatings.txt")
        myRatingsRDD = self.sc.parallelize(myRatings, 1)

        # load ratings and movie titles

        # ratings is an RDD of (last digit of timestamp, (userId, movieId, rating))
        ratings = self.sc.textFile(Parameters.data_path+"ratings.csv").map(self.parseRating)

        # movies is an RDD of (movieId, movieTitle)

        try:
            self.movies = dict(self.sc.textFile(Parameters.data_path+"movies.csv").map(self.parseMovie).collect())
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

        #.union(myRatingsRDD) \
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
        ranks = [8, 12]
        """Regularization parameter.(default: 0.01)"""
        lambdas = [0.1, 10.0]
        """Number of iterations of ALS. (default: 5) Yap˝lacaksa maximum 20 yap˝lmal˝ ."""
        numIters = Parameters.iteration_array

        self.bestModel = None
        self.bestValidationRmse = float("inf")
        bestRank = 0
        bestLambda = -1.0
        bestNumIter = -1

        for rank, lmbda, numIter in itertools.product(ranks, lambdas, numIters):
            # Build the recommendation model using Alternating Least Squares
            model = ALS.train(training, rank, numIter, lmbda)
            validationRmse = self.computeRmse(model, validation, numValidation)
            print("RMSE (validation) = %f for the model trained with " % validationRmse + \
                  "rank = %d, lambda = %.1f, and numIter = %d." % (rank, lmbda, numIter))
            if (validationRmse < self.bestValidationRmse):
                bestModel = model
                bestValidationRmse = validationRmse
                bestRank = rank
                bestLambda = lmbda
                bestNumIter = numIter

        testRmse = self.computeRmse(self.bestModel, test, numTest)

        # evaluate the best model on the test set
        print("The best model was trained with rank = %d and lambda = %.1f, " % (bestRank, bestLambda) \
              + "and numIter = %d, and its RMSE on the test set is %f." % (bestNumIter, testRmse))

        # compare the best model with a naive baseline that always returns the mean rating
        meanRating = training.union(validation).map(lambda x: x[2]).mean()
        baselineRmse = sqrt(test.map(lambda x: (meanRating - x[2]) ** 2).reduce(add) / numTest)
        improvement = (baselineRmse - testRmse) / baselineRmse * 100
        print("The best model improves the baseline by %.2f" % (improvement) + "%.")

        # make personalized recommendations





