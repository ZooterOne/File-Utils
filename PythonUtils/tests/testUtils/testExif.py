import unittest

from parameterized import parameterized # type: ignore
from PIL import Image
from typing import Tuple

from utils import exif


class TestExif(unittest.TestCase):
    @parameterized.expand([('tests/assets/duplicates/four.png', None),
                           ('tests/assets/duplicates/five.png', '1985:10:26 01:20:00')])
    def testGetDateTime(self, filePath: str, dateTime: str | None) -> None:
        with Image.open(filePath) as image:
            exifData = image.getexif()
            self.assertEqual(exif.getDateTime(exifData), dateTime)

    @parameterized.expand([('tests/assets/duplicates/four.png', None),
                           ('tests/assets/duplicates/five.png', (34.141525064364686, -118.34978513740731))])
    def testGetLocation(self, filePath: str, location: Tuple[float, float] | None) -> None:
        with Image.open(filePath) as image:
            exifData = image.getexif()
            self.assertEqual(exif.getLocation(exifData), location)

    @parameterized.expand([((43.582899339817914, 7.129233586685825), 'Antibes'),
                           ((-27.465981879167295, 153.0239650910338), 'Brisbane')])
    def testGetLocationName(self, location: Tuple[float, float], locationName: str) -> None:
        self.assertEqual(exif.getLocationName(location), locationName)

    def testGenerateGroupName(self) -> None:
        self.assertDictEqual(exif.generateGroupNames('tests/assets', False, False, None, lambda: None), {})
        groupsNoLocation = exif.generateGroupNames('tests/assets', True, False, None, lambda: None)
        expectedGroupsNoLocation = {'1985-10-26': ['tests/assets/duplicates/five.png']}
        self.assertDictEqual(groupsNoLocation, expectedGroupsNoLocation)
        groupsWithLocation = exif.generateGroupNames('tests/assets', True, True, 'UNDEFINED', lambda: None)
        expectedGroupsWithLocation = {'UNDEFINED': ['tests/assets/one.png', 'tests/assets/three.png',
                                                    'tests/assets/two.png', 'tests/assets/duplicates/four.png'],
                                      '1985-10-26-Universal City': ['tests/assets/duplicates/five.png']}
        self.assertDictEqual(groupsWithLocation, expectedGroupsWithLocation)