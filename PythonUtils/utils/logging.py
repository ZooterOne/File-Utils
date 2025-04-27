import json
import logging.config


def setupBasicLogging(outputPath: str, level: str) -> None:
    '''Setup logging using given output file and logging level.'''
    loggingConfig = {
        "version": 1,
        "disable_existing_loggers": False,
        "formatters": {
            "simple": {
                "format": "%(asctime)s : %(levelname)s : %(message)s"
            }
        },
        "handlers": {
            "file": {
                "class": "logging.FileHandler",
                "formatter": "simple",
                "filename": f"{outputPath}"
            }
        },
        "loggers": {
            "root": {
                "level": f"{level}",
                "handlers": ["file"]
            }
        }
    }
    logging.config.dictConfig(config=loggingConfig)


def setupWithConfigFile(configPath: str) -> None:
    '''Setup logging using given config json file.'''
    with open(configPath) as configFile:
        loggingConfig = json.load(configFile)
    logging.config.dictConfig(config=loggingConfig)
