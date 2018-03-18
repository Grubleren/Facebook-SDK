// Win32Project1.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "time.h"

extern "C"
{
	__declspec(dllexport) clock_t _cdecl cputime()
	{
		clock_t t=clock();

		return t;
	}
}