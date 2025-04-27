import os
import shutil
import unittest

from utils import logger, logging


class TestLogging(unittest.TestCase):
    def setUp(self) -> None:
        os.makedirs('__tmp__')

    def tearDown(self) -> None:
        shutil.rmtree('__tmp__')

    def testBasicConfig(self):
        logging.setupBasicLogging('__tmp__/test.log', 'INFO')
        self.assertHelper()

    def testConfigFile(self):
        logging.setupWithConfigFile('tests/assets/logging.json')
        self.assertHelper()

    def assertHelper(self):
        infoMessage = 'This message is logged.'
        debugMessage = 'This message is not logged.'
        logger.info(infoMessage)
        logger.debug(debugMessage)
        self.assertTrue(os.path.exists('__tmp__/test.log'))
        with open('__tmp__/test.log', 'r') as logFile:
            logFileContent = logFile.read()
            self.assertIn(infoMessage, logFileContent)
            self.assertNotIn(debugMessage, logFileContent)
