from logging import getLogger

from .logging import setupBasicLogging, setupWithConfigFile
from .files import generateChecksum, parseFilesInDirectory
from .progressSpinner import ProgressSpinner
from .duplicates import findDuplicatesInDirectory, findDuplicatesInDirectories
from .exif import getDateTime, getLocation, getLocationName, generateGroupNames


# Regex matching photos filename
PHOTOS_REGEX_FILTER = r"\.(([jJ][pP][eE]?[gG])|([pP][nN][gG])|([hH][eE][iI][cC]))$"


all = (setupBasicLogging, setupWithConfigFile,
       generateChecksum, parseFilesInDirectory,
       ProgressSpinner,
       findDuplicatesInDirectory, findDuplicatesInDirectories,
       getDateTime, getLocation, getLocationName, generateGroupNames)


logger = getLogger(__name__)
