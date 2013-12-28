// Copyright © 2006 Microsoft Corporation.  All Rights Reserved

#pragma once

__declspec( dllexport )
int __stdcall MessageHookProc(int nCode, WPARAM wparam, LPARAM lparam);

namespace ManagedInjector {

	public ref class Injector: System::Object {

	public:
		static void Inject(System::IntPtr windowHandle, System::String^ injectDllName, System::String^  assemblyFileName, System::String^ className, System::String^ methodName);
	};
}