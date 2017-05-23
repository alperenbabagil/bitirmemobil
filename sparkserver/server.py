import time, sys, cherrypy, os
from paste.translogger import TransLogger

import glo
from app import create_app


os.environ['JAVA_HOME']="C:\\Program Files (x86)\\Java\\jdk1.7.0_55"

# os.environ["SPARK_HOME"] = "F:\\bitirme\\spark-2.0.1-bin-hadoop2.7"
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python")
# sys.path.append("F:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python\\lib\\py4j-0.10.3-src.zip")

# os.environ["SPARK_HOME"] = "C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7"
# sys.path.append("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7\\python")
# sys.path.append("C:\\Users\\alperen\\Desktop\\bitirmeyeni\\spark-2.1.0-bin-hadoop2.7\\python\\lib\\py4j-0.10.4-src.zip")
#
#

import Parameters

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


def init_spark_context():
    # sc = SparkContext('local')
    # words = sc.parallelize(["scala", "java", "hadoop", "spark", "akka"])
    # print(words.count())

    # load spark context
    try:
        #conf = SparkConf().setAppName("movie_recommendation-server").set("spark.executor.memory", "2g")
        # conf = SparkConf().setAppName("movie_recommendation-server").setExecutorEnv('spark.executor.memory','2g')
        #sc = SparkContext(conf=conf, pyFiles=['recomm_engine.py', 'app.py'])

        conf = SparkConf() \
                .setAppName("MovieLensALS") \
            .set("spark.executor.memory", "2g")

        sc = SparkContext(conf=conf)

    except Exception as e:
        print(e)
    # IMPORTANT: pass aditional Python modules to each worker

 
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

    # Init spark context and load libraries
    sc = init_spark_context()
    #sc = None

    # dataset_path = os.path.join('datasets', 'ml-latest')
    #dataset_path = "F:\\bitirme\\bilal\\son\\MLlibSpark"
    dataset_path = Parameters.data_path
    #app = create_app(None,dataset_path)
    app = create_app(sc,dataset_path)
    #app = create_app(None,None)

    # start web server
    run_server(app)

