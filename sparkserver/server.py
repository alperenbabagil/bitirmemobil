import time, sys, cherrypy, os
import Parameters

from paste.translogger import TransLogger
from app import create_app

os.environ['PYSPARK_PYTHON'] = "python3"
os.environ['JAVA_HOME'] = Parameters.java_home



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


def init_spark_context():
    # sc = SparkContext('local')
    # words = sc.parallelize(["scala", "java", "hadoop", "spark", "akka"])
    # print(words.count())

    sc=None
    # load spark context
    try:
        conf = SparkConf().setAppName("movie_recommendation-server").set("spark.executor.memory", "4g")
        sc = SparkContext(conf=conf, pyFiles=['recomm_engine.py', 'app.py'])
        sc.setCheckpointDir(Parameters.checkpoint_path)
    except Exception as e:
        print(e)
    return sc


def run_server(app):
    # Enable WSGI access logging via Paste
    app_logged = TransLogger(app)

    # Mount the WSGI callable object (app) on the root directory
    cherrypy.tree.graft(app_logged, '/')

    # Set the configuration of the web server
    cherrypy.config.update({
        'engine.autoreload.on': True,
        'log.screen': True,
        'server.socket_port': 5432,
        'server.socket_host': '0.0.0.0'
    })

    # Start the CherryPy WSGI web server
    cherrypy.engine.start()
    cherrypy.engine.block()


if __name__ == "__main__":

    # Init spark context
    sc = init_spark_context()
    app = create_app(sc)
    # start web server
    run_server(app)

