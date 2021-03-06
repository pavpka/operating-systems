
#include "pch.h"
#include <windows.h>
#include <iostream>
#include <string>

using namespace std;

struct ArrayStruct
{
	volatile char *array;
	int size;
	int index;
	CRITICAL_SECTION cs;
	HANDLE hWorkSemaphore;
};

DWORD WINAPI work(ArrayStruct *arrayStruct) {
	EnterCriticalSection(&arrayStruct->cs);
	char *left = new char[arrayStruct->size];
	char *right = new char[arrayStruct->size];
	int l = 0, r = 0;
	char *result = new char[arrayStruct->size]{ 0 };

	int sleep;

	cout << "Please time interval: ";
	cin >> sleep;
	cout << "new array: " << endl;
	for (int i = 0; i < arrayStruct->size; i++) {
		if (int(arrayStruct->array[i]) > 47 && int(arrayStruct->array[i]) < 58) {
			left[l++] = arrayStruct->array[i];
		}
		else { right[r++] = '_'; 
		}
	}
	for (int i = 0; i < arrayStruct->size; i++) {
		if (i < l) {
			arrayStruct->array[i] = left[i];
		}
		else arrayStruct->array[i] = right[i-l];

		ReleaseSemaphore(arrayStruct->hWorkSemaphore, 1, NULL);
		Sleep(sleep * 1000);
	}

	LeaveCriticalSection(&arrayStruct->cs);
	return 0;
}

DWORD WINAPI sumElement(ArrayStruct *arrayStruct) {
	Sleep(1000);
	EnterCriticalSection(&arrayStruct->cs);

	int sum = 0;
		for (int i = 0; i < arrayStruct->size; i++) {
			sum += int(arrayStruct->array[i]);
		}
	cout << endl << "Sum = " << sum << endl;

	LeaveCriticalSection(&arrayStruct->cs);
	ReleaseSemaphore(arrayStruct->hWorkSemaphore, 1, NULL);
	return 0;
}

int main() {
	HANDLE hWork, hMult;
	DWORD IDWork, IDMult;

	ArrayStruct arrayStruct;
	arrayStruct.index = -1;

	cout << "Please enter size of array: ";
	cin >> arrayStruct.size;

	volatile char *array;
	array = new char[arrayStruct.size];
	string elements;
	cout << "Please enter " << arrayStruct.size << " elements of array in a row: ";
	cin >> elements;
	for (int i = 0; i < arrayStruct.size; i++) {
		array[i] = elements[i];
	}
	arrayStruct.array = array;

	cout << "Source array: " << endl;
	for (int i = 0; i < arrayStruct.size; i++) {
		cout << array[i]<< " ";
	}
	cout << endl;

	arrayStruct.hWorkSemaphore = CreateSemaphore(NULL, 0, 1, L"hWorkSemaphore");
	InitializeCriticalSection(&arrayStruct.cs);

	hWork = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)work, &arrayStruct, 0, &IDWork);
	hMult = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)sumElement, &arrayStruct, 0, &IDMult);
	
	for (int i = 0; i < arrayStruct.size; i++) {
		WaitForSingleObject(arrayStruct.hWorkSemaphore, INFINITE);
		cout <<" "<< array[i] << endl;
		cout.flush();
	}
	WaitForSingleObject(arrayStruct.hWorkSemaphore, INFINITE);

	DeleteCriticalSection(&arrayStruct.cs);
	CloseHandle(arrayStruct.hWorkSemaphore);
	CloseHandle(hMult);
	CloseHandle(hWork);
	return 0;
}
