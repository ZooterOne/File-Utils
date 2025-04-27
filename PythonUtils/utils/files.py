import hashlib
import os

import utils

from typing import Callable


def generateChecksum(filePath: str) -> str:
    '''Generate the checksum for a given file.'''
    with open(filePath, "rb") as file:
        hash = hashlib.file_digest(file, "sha256")
        return hash.hexdigest()


def parseFilesInDirectory(directoryPath: str, recursive: bool, 
                          callback: Callable[[str], None]) -> None:
    '''Parse each file in a given directory.'''
    utils.logger.info(f'Parsing files in directory {directoryPath}.')
    for root, _, files in os.walk(directoryPath):
        utils.logger.debug(f'Parsing directory {root}.')
        for file in files:
            filePath = os.path.join(root, file)
            utils.logger.debug(f'Running callback function on {filePath}.')
            callback(filePath)
        if not recursive:
            break
