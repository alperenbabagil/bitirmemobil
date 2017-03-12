import time, sys, cherrypy, os
from paste.translogger import TransLogger
from app import create_app


os.environ["SPARK_HOME"] = "D:\\bitirme\\spark-2.0.1-bin-hadoop2.7"

sys.path.append("D:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python")
sys.path.append("D:\\bitirme\\spark-2.0.1-bin-hadoop2.7\\python\\lib\\py4j-0.10.3-src.zip")

try:
    from pyspark import SparkContext
    from pyspark import SparkConf
    from pyspark.mllib.recommendation import ALS
    print("success")

except ImportError as exx:
    print(exx)


def init_spark_context():
    # load spark context
    conf = SparkConf().setAppName("movie_recommendation-server")
    # IMPORTANT: pass aditional Python modules to each worker
    sc = SparkContext(conf=conf, pyFiles=['engine.py', 'app.py'])
 
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
    # dataset_path = os.path.join('datasets', 'ml-latest')
    dataset_path = "D:\\bitirme\\bilal\\son\\MLlibSpark"
    app = create_app(sc,None)
 
    # start web server
    run_server(app)

