#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <windows.h>

#include <delayimp.h>
#include <string.h>
#include <cstdint>

// allow renaming of node.exe
static FARPROC WINAPI load_node_hook(uint32_t event, PDelayLoadInfo info)
{
  // only handle load of node.exe
  if (event != dliNotePreLoadLibrary || _stricmp(info->szDll, "node.exe") != 0)
  {
    return nullptr;
  }

  // if node.exe is expected, provide handle to calling process
  HMODULE m = GetModuleHandle(nullptr);
  return (FARPROC)m;
}

decltype(__pfnDliNotifyHook2) __pfnDliNotifyHook2 = load_node_hook;