import argparse
import datetime
import textwrap
import shutil

import utils

from pathlib import Path
from pillow_heif import register_heif_opener # type: ignore
from typing import Dict, List


def parseArgs() -> argparse.Namespace:
    '''Parse command line arguments.'''
    parser = argparse.ArgumentParser(prog='SortPhotos', 
                                     description='Sort photos into folders named using date and location from photo Exif data.',
                                     formatter_class=argparse.RawDescriptionHelpFormatter,
                                     epilog=textwrap.dedent('''Examples:
        # Move photos in the current folder in folder ./Sorted, and use photo date to sort them into folders
        SortPhotos
        # Move photos in ~/Pictures in folder ~/Pictures/Output, and use photo date and location to sort them into folders
        SortPhotos -d ~/Pictures -o ~/Pictures/Output --location
        # Copy photos in ~/Pictures in folder ./Sorted, and use phot date to sort them into folders
        SortPhotos -d ~/Pictures --copy
                                                            '''))
    parser.add_argument('-d', '--directory', type=str, default='./', help='The path of the directory to process.')
    parser.add_argument('-o', '--output', type=str, default='./SORTED', help='The path of the folder to output sorted photos into.')
    parser.add_argument('--location', action='store_true', help='Use gps location in adding to the date to generate folder names.')
    parser.add_argument('--undefined', type=str, default='UNDEFINED', help='The folder name to use when data cannot be extracted from photo.')
    parser.add_argument('--copy', action='store_true', help='Copy the photos instead of moving them.')
    parser.add_argument('--debug', action='store_true', help='Generate a debug log file.')
    return parser.parse_args()


def processGroup(groupName: str, group: List[str], outputPath: Path, copy: bool) -> None:
    '''Generate group folder and copy/move photos into it.'''
    for photo in group:
        photoPath = Path(photo)
        if copy:
            shutil.copy(photoPath, outputPath / groupName / photoPath.name)
        else:
            photoPath.replace(outputPath / groupName / photoPath.name)


def processUndefinedGroup(groupName: str, group: List[str], outputPath: Path, copy: bool) -> None:
    '''Generate undefined group folder and copy/move photos into it, using creation time to sort them.'''
    for photo in group:
        photoPath = Path(photo)
        modificationTime = photoPath.stat().st_mtime
        subGroup = datetime.datetime.fromtimestamp(modificationTime).strftime('%Y-%m-%d')
        outputDirectoryPath = Path(outputPath / groupName / subGroup)
        outputDirectoryPath.mkdir(parents=True, exist_ok=True)
        if copy:
            shutil.copy(photoPath, outputDirectoryPath / photoPath.name)
        else:
            photoPath.replace(outputDirectoryPath / photoPath.name)


def processGroups(groups: Dict[str, List[str]], directory: str, undefinedGroup: str, copy: bool) -> None:
    '''Generate all group folders and copy/move photos into them.'''
    outputPath = Path(directory)
    for groupName in groups:
        Path(outputPath / groupName).mkdir(parents=True, exist_ok=True)
        if (groupName == undefinedGroup):
            processUndefinedGroup(groupName, groups[groupName], outputPath, copy)
        else:
            processGroup(groupName, groups[groupName], outputPath, copy)


def showMessage(message: str) -> None:
    '''Show and log message.'''
    print(message)
    utils.logger.info(message)


def run() -> None:
    '''Run the command line tool.'''
    print('''
┏┓     ┏┓┓       
┗┓┏┓┏┓╋┃┃┣┓┏┓╋┏┓┏
┗┛┗┛┛ ┗┣┛┛┗┗┛┗┗┛┛
''')

    args = parseArgs()

    if not Path(args.directory).exists():
        print(f'Directory {args.directory} does not exist.')
        exit(-1)
    Path(args.output).mkdir(parents=True, exist_ok=True)

    if (args.debug):
        debugPath = Path(args.output) / 'debug.log'
        utils.setupBasicLogging(str(debugPath), 'DEBUG')
    showMessage(f'Sorting photos in directory {args.directory}.')
    
    register_heif_opener()
    spinner = utils.ProgressSpinner('Processing files ')

    groupedPhotos = utils.exif.generateGroupNames(args.directory, True, args.location, args.undefined, lambda: spinner.update())
    processGroups(groupedPhotos, args.output, args.undefined, args.copy)

    spinner.finish()
    sortedCount = sum([len(groupedPhotos[key]) for key in groupedPhotos])
    showMessage(f'{sortedCount} photos sorted into {len(groupedPhotos)} folders.')
    

if __name__ == '__main__':
    run()
