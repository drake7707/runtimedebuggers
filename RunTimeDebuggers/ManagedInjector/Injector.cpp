// Copyright © 2006 Microsoft Corporation.  All Rights Reserved
// Taken from https://cracknetproject.codeplex.com/, but changed it a bit

#include "stdafx.h"

#include "Injector.h"
#include <vcclr.h>

using namespace ManagedInjector;

static unsigned int WM_GOBABYGO = ::RegisterWindowMessage(L"Injector_GOBABYGO!");
static HHOOK _messageHookHandle;

//-----------------------------------------------------------------------------
//Spying Process functions follow
//-----------------------------------------------------------------------------
void Injector::Inject(System::IntPtr windowHandle, System::String^ injectDllName, System::String^  assemblyFileName, System::String^ className, System::String^ methodName) {

	System::String^ assemblyClassAndMethod = assemblyFileName + "$" + className + "$" + methodName;
	pin_ptr<const wchar_t> acmLocal = PtrToStringChars(assemblyClassAndMethod);

	pin_ptr<const wchar_t> injectDllNameLocal = PtrToStringChars(injectDllName);
	
	HINSTANCE hinstDLL = ::LoadLibrary(injectDllNameLocal); 

	if (hinstDLL)
	{
		DWORD processID = 0;
		DWORD threadID = ::GetWindowThreadProcessId((HWND)windowHandle.ToPointer(), &processID);

		if (processID)
		{
			HANDLE hProcess = ::OpenProcess(PROCESS_ALL_ACCESS, FALSE, processID);
			if (hProcess)
			{
				int buffLen = (assemblyClassAndMethod->Length + 1) * sizeof(wchar_t);
				void* acmRemote = ::VirtualAllocEx(hProcess, NULL, buffLen, MEM_COMMIT, PAGE_READWRITE);

				if (acmRemote)
				{
					::WriteProcessMemory(hProcess, acmRemote, acmLocal, buffLen, NULL);
				
					HOOKPROC procAddress = (HOOKPROC)GetProcAddress(hinstDLL, "MessageHookProc");
					_messageHookHandle = ::SetWindowsHookEx(WH_CALLWNDPROC, procAddress, hinstDLL, threadID);

					if (_messageHookHandle)
					{
						::SendMessage((HWND)windowHandle.ToPointer(), WM_GOBABYGO, (WPARAM)acmRemote, 0);
						::UnhookWindowsHookEx(_messageHookHandle);
					}

					::VirtualFreeEx(hProcess, acmRemote, buffLen, MEM_RELEASE);
				}

				::CloseHandle(hProcess);
			}
		}
		::FreeLibrary(hinstDLL);
	}
}


void ShowError(System::String^ errmsg) {
	System::Windows::Forms::MessageBox::Show(errmsg, "Error");
}

__declspec( dllexport ) 
int __stdcall MessageHookProc(int nCode, WPARAM wparam, LPARAM lparam) {

	if (nCode == HC_ACTION) 
	{
		CWPSTRUCT* msg = (CWPSTRUCT*)lparam;
		if (msg != NULL && msg->message == WM_GOBABYGO)
		{
			wchar_t* acmRemote = (wchar_t*)msg->wParam;

			System::String^ acmLocal = gcnew System::String(acmRemote);
			cli::array<System::String^>^ acmSplit = acmLocal->Split('$');

			try 
			{
				System::Reflection::Assembly^ assembly = System::Reflection::Assembly::LoadFile(acmSplit[0]);
				if (assembly != nullptr)
				{
					System::Type^ type = assembly->GetType(acmSplit[1]);
					if (type != nullptr)
					{
						System::Reflection::MethodInfo^ methodInfo = type->GetMethod(acmSplit[2], System::Reflection::BindingFlags::Static | System::Reflection::BindingFlags::Public);
						if (methodInfo != nullptr)
						{
							methodInfo->Invoke(nullptr, nullptr);
						}
						else
							ShowError("Unable to determine method " + acmSplit[2]);
					}
					else
						ShowError("Unable to determine type " + acmSplit[1]);
				}
				else
					ShowError("Unable to load assembly " + acmSplit[2]);
			}
			catch(System::Exception^ e)
			{
				ShowError("Unable to load assembly '" + acmSplit[0] + "': " + e->GetType()->ToString() + " - " + e->Message);
			}
		}
	}
	return CallNextHookEx(_messageHookHandle, nCode, wparam, lparam);
}
