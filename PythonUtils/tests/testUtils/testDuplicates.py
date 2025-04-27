import re
import unittest

from utils import duplicates


class TestDuplicates(unittest.TestCase):
    def testDirectoryChecksums(self) -> None:
        regexFilter = re.compile(r"\.png$")
        checksums = duplicates.generateDirectoryChecksums('tests', True, regexFilter, lambda: None)
        expectedChecksums = {'0fc39b203f47463fa651e07e03fd758f5086bafa75b84aca4920dcb13f412702': ['tests/assets/one.png', 'tests/assets/duplicates/four.png'],
                             '1e916b0d4ac220d218733a789810129c0d7cdbe4803fe6825abbf76f94bdc248': ['tests/assets/two.png'],
                             '8cc93a243607d9f0557c7bd33c190a0eae9abb1a4b99d45cab9a12ac6280ae09': ['tests/assets/three.png'],
                             'ae1b06c3e37fa3b5373b27c9841ec2723e69529c8099aa92fca8d41a59ec7360': ['tests/assets/duplicates/five.png']}
        self.assertDictEqual(checksums, expectedChecksums)

    def testMergeDirectoryChecksums(self) -> None:
        firstChecksums = {'aa': ['one', 'two'], 'bb': ['one']}
        secondChecksums = {'bb': ['two'], 'cc': ['one']}
        checksums = duplicates.mergeDirectoryChecksums(firstChecksums, secondChecksums)
        expectedChecksums = {'aa': ['one', 'two'], 'bb': ['one', 'two'], 'cc': ['one']}
        self.assertDictEqual(checksums, expectedChecksums)

    def testGenerateDuplicatesFromDirectoryChecksums(self) -> None:
        directoryChecksums = {'aa': ['one', 'two'], 'bb': ['three'], 'cc': ['four', 'four']}
        duplicatesCollection = duplicates.generateDuplicatesFromDirectoryChecksums(directoryChecksums)
        expectedDuplicates = [['one', 'two']]
        self.assertListEqual(duplicatesCollection, expectedDuplicates)

    def testFindDuplicatesInDirectory(self) -> None:
        regexFilter = re.compile(r"\.png$")
        duplicatePngs = duplicates.findDuplicatesInDirectory('tests', True, regexFilter, lambda: None)
        expectedDuplicates = [['tests/assets/one.png', 'tests/assets/duplicates/four.png']]
        self.assertListEqual(duplicatePngs, expectedDuplicates)

    def testFindDuplicatesInDirectories(self) -> None:
        regexFilter = re.compile(r"\.png$")
        self.assertListEqual(duplicates.findDuplicatesInDirectories(False, regexFilter, lambda: None), [])
        duplicatePngs = duplicates.findDuplicatesInDirectories(False, regexFilter, lambda: None, 'tests/assets', 'tests/assets/duplicates')
        expectedDuplicates = [['tests/assets/one.png', 'tests/assets/duplicates/four.png']]
        self.assertListEqual(duplicatePngs, expectedDuplicates)
        duplicatePngs = duplicates.findDuplicatesInDirectories(True, regexFilter, lambda: None, 'tests/assets', 'tests/assets/duplicates')
        self.assertListEqual(duplicatePngs, expectedDuplicates)
