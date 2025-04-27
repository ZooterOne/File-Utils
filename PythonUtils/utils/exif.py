import re
import reverse_geocoder as rg # type: ignore

import utils

from PIL import Image
from typing import Callable, Dict, List, Tuple


# Required Exif & Location tags
DATETIME_TAG = 0x0132
EXIF_TAG = 0x8769
DATETIME_ORIGINAL_TAG = 0x9003
GPSINFO_TAG = 0x8825
GPS_LATITUDE_REF = 1
GPS_LATITUDE = 2
GPS_LONGITUDE_REF = 3
GPS_LONGITUDE = 4
REVERSE_GEOCODE_LOCATION_TAG = 'name'


def _DegreesMinutesSeconds2DecimalDegrees(value: Tuple[float, float, float], valueRef: str) -> float:
    '''Convert longitude/latitude gps values from Degrees/Minutes/Seconds to Decimal Degrees.'''
    valueInDegrees = value[0] + value[1] / 60.0 + value[2] / 3600.0
    if valueRef == 'S' or valueRef == 'W':
        return -valueInDegrees
    return valueInDegrees


def getDateTime(exifData: Image.Exif) -> str | None:
    '''Get date & time from exif data.'''
    extendedExifData = exifData.get_ifd(EXIF_TAG)
    if DATETIME_ORIGINAL_TAG in extendedExifData:
        return extendedExifData[DATETIME_ORIGINAL_TAG]
    elif DATETIME_TAG in exifData:
        return exifData[DATETIME_TAG]
    return None


def getLocation(exifData: Image.Exif) -> Tuple[float, float] | None:
    '''Get latitude/longitude from exif data.'''
    gpsData = exifData.get_ifd(GPSINFO_TAG)
    if all(k in gpsData for k in [GPS_LATITUDE_REF, GPS_LATITUDE, 
                                  GPS_LONGITUDE_REF, GPS_LONGITUDE]):
            latitude = _DegreesMinutesSeconds2DecimalDegrees(gpsData[GPS_LATITUDE], gpsData[GPS_LATITUDE_REF])
            longitude = _DegreesMinutesSeconds2DecimalDegrees(gpsData[GPS_LONGITUDE], gpsData[GPS_LONGITUDE_REF])
            return (latitude, longitude)
    return None
                    

def getLocationName(location: Tuple[float, float]) -> str | None:
    '''Get location name from latitude/longitude.'''
    gpsLocation = rg.search(location, verbose=False)
    if len(gpsLocation) == 1 and REVERSE_GEOCODE_LOCATION_TAG in gpsLocation[0]:
        return gpsLocation[0][REVERSE_GEOCODE_LOCATION_TAG]
    return None


def generateGroupNames(directoryPath: str, recursive: bool, useLocation: bool,
                       defaultGroup: str | None, progressCallback: Callable[[], None]) -> Dict[str, List[str]]:
    '''Generate the group name from Exif data for each image file in a given directory.'''
    regexFilter = re.compile(utils.PHOTOS_REGEX_FILTER)
    groups: dict[str, list[str]] = {}

    def fileCallback(filePath: str) -> None:
        if regexFilter.search(filePath):
            fullDateTime = None
            location = None
            with Image.open(filePath) as image:
                exifData = image.getexif()
                fullDateTime = utils.getDateTime(exifData)
                if fullDateTime:
                    utils.logger.debug(f'DateTime for {filePath}: {fullDateTime}.')
                if useLocation:
                    gpsLocation = utils.getLocation(exifData)
                    if gpsLocation:
                        utils.logger.debug(f'Latitude for {filePath}: {gpsLocation[0]}.')
                        utils.logger.debug(f'Longitude for {filePath}: {gpsLocation[1]}.')
                        location = utils.getLocationName(gpsLocation)
                        if location:
                            utils.logger.debug(f'Location for {filePath}: {location}.')
            groupName = None
            if fullDateTime:
                groupName = '-'.join(fullDateTime.split()[0].split(':'))
            if location:
                groupName = '-'.join([groupName, location]) if groupName else location
            if not groupName:
                groupName = defaultGroup
            if groupName:
                if groupName not in groups:
                    groups[groupName] = []
                groups[groupName].append(filePath)
                utils.logger.debug(f'Group for {filePath}: {groupName}.')
        progressCallback()

    utils.files.parseFilesInDirectory(directoryPath, recursive, fileCallback)
    return groups
