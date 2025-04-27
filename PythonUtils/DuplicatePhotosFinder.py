import argparse
import csv
import os
import re
import textwrap

import utils

from typing import List


def parseArgs() -> argparse.Namespace:
    '''Parse command line arguments.'''
    parser = argparse.ArgumentParser(prog='DuplicatePhotosFinder', 
                                     description='Find & report duplicate photos.',
                                     formatter_class=argparse.RawDescriptionHelpFormatter,
                                     epilog=textwrap.dedent('''Examples:
        # Find duplicate photos in the current folder and generate duplicates.csv report file
        DuplicatePhotosFinder
        # Find duplicate photos in ~/Pictures and generate pictures.csv report file
        DuplicatePhotosFinder -d ~/Pictures -o pictures.csv
                                                            '''))
    parser.add_argument('-d', '--directory', type=str, default='./', help='The path of the directory to process.')
    parser.add_argument('-o', '--output', type=str, default='./duplicates.csv', help='The path of the output csv file to generate.')
    parser.add_argument('--debug', action='store_true', help='Generate a debug log file.')
    return parser.parse_args()


def generateCSVOutput(filePath: str, duplicates: List[List[str]], spinner: utils.ProgressSpinner) -> None:
    '''Generate CSV output file from duplicate collections.'''
    utils.logger.info(f'Generating CSV file {filePath}.')
    spinner.message = 'Generating output file '
    with open(filePath, 'w', newline='') as file:
        csvWriter = csv.writer(file, delimiter=',', lineterminator='\n')
        csvWriter.writerow(['Original file', 'Duplicate file'])
        for duplicate in duplicates:
            row = ['', '']
            csvWriter.writerow(row)
            duplicateIter = iter(duplicate)
            row = [next(duplicateIter), next(duplicateIter)]
            utils.logger.debug(f'Writing CSV row: {row}.')
            csvWriter.writerow(row)
            for entry in duplicateIter:
                row = ['', entry]
                utils.logger.debug(f'Writing CSV row: {row}.')
                csvWriter.writerow(row)
            spinner.update()


def showMessage(message: str) -> None:
    '''Show and log message.'''
    print(message)
    utils.logger.info(message)


def run() -> None:
    '''Run the command line tool.'''
    print('''
┳┓    ┓•      ┏┓┓       ┏┓•   ┓    
┃┃┓┏┏┓┃┓┏┏┓╋┏┓┃┃┣┓┏┓╋┏┓┏┣ ┓┏┓┏┫┏┓┏┓
┻┛┗┻┣┛┗┗┗┗┻┗┗ ┣┛┛┗┗┛┗┗┛┛┻ ┗┛┗┗┻┗ ┛ 
    ┛                              
''')

    args = parseArgs()

    if not os.path.exists(args.directory):
        print(f'Directory {args.directory} does not exist.')
        exit(-1)
    outputPath = os.path.splitext(os.path.basename(args.output))
    if not outputPath[0] or not outputPath[1]:
        print(f'Incorrect output filename {args.output}.')
        exit(-1)
    outputDirectory = os.path.dirname(args.output)
    if outputDirectory and not os.path.exists(outputDirectory):
        print(f'Directory {outputDirectory} does not exist.')
        exit(-1)

    if (args.debug):
        debugPath = os.path.splitext(args.output)
        utils.setupBasicLogging(debugPath[0] + '.log', 'DEBUG')
    showMessage(f'Searching duplicate photos in directory {args.directory}.')
    
    regexFilter = re.compile(utils.PHOTOS_REGEX_FILTER)
    spinner = utils.ProgressSpinner('Processing files ')

    duplicates = utils.duplicates.findDuplicatesInDirectory(args.directory, True, regexFilter, lambda: spinner.update())
    
    duplicateCount = sum([len(items) for items in duplicates])
    spinner.finish()
    showMessage(f'Number of duplicate photos found: {duplicateCount}')
    
    generateCSVOutput(args.output, duplicates, spinner)
    
    spinner.finish()
    showMessage(f'Output CSV file {args.output} generated.')
    

if __name__ == '__main__':
    run()
