import time

from progress.spinner import PixelSpinner # type: ignore


# Time in seconds between spinner updates
SPINNER_UPDATE_DELTA = 0.5


class ProgressSpinner:
    '''Defines a spinner to show progress during process.'''
    def __init__(self, message: str) -> None:
        '''Initialize the spinner with initial message.'''
        self.__spinner = PixelSpinner(message)
        self.__lastUpdate: float = 0

    @property
    def message(self) -> str:
        '''Get the currently displayed message.'''
        return self.__spinner.message
    
    @message.setter
    def message(self, value: str) -> None:
        '''Set the message to display.'''
        self.__spinner.message = value
        self.__spinner.update()

    def update(self) -> None:
        '''Update the spinner. If update interval is too small, the display won't refresh.'''
        self.__lastUpdate = time.monotonic() if self.__lastUpdate == 0 else self.__lastUpdate
        currentUpdate = time.monotonic()
        if currentUpdate - self.__lastUpdate > SPINNER_UPDATE_DELTA:
            self.__spinner.next()
            self.__lastUpdate = 0

    def finish(self) -> None:
        '''Terminate the spinner.'''
        self.__spinner.finish()
