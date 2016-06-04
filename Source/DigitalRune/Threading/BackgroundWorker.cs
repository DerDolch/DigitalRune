﻿#region ----- Copyright -----
/*
  The class in this file is based on the BackgroundWorker from the ParallelTasks library (see 
  http://paralleltasks.codeplex.com/) which is licensed under Ms-PL (see below).


  Microsoft Public License (Ms-PL)

  This license governs use of the accompanying software. If you use the software, you accept this 
  license. If you do not accept the license, do not use the software.

  1. Definitions

  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
  meaning here as under U.S. copyright law.

  A "contribution" is the original software, or any additions or changes to the software.

  A "contributor" is any person that distributes its contribution under this license.

  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

  2. Grant of Rights

  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
  distribute its contribution or any derivative works that you create.

  (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
  otherwise dispose of its contribution in the software or derivative works of the contribution in 
  the software.

  3. Conditions and Limitations

  (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
  logo, or trademarks.

  (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
  by the software, your patent license from such contributor to the software ends automatically.

  (C) If you distribute any portion of the software, you must retain all copyright, patent, 
  trademark, and attribution notices that are present in the software.

  (D) If you distribute any portion of the software in source code form, you may do so only under 
  this license by including a complete copy of this license with your distribution. If you 
  distribute any portion of the software in compiled or object code form, you may only do so under a 
  license that complies with this license.

  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
  express warranties, guarantees or conditions. You may have additional consumer rights under your 
  local laws which this license cannot change. To the extent permitted under your local laws, the 
  contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
  and non-infringement.  
*/
#endregion

#if !NETFX_CORE && !PORTABLE && !USE_TPL
using System.Collections.Generic;
using System.Threading;


namespace DigitalRune.Threading
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "BackgroundWorkers are kept alive and can only be reclaimed by garbage collector.")]
  internal sealed class BackgroundWorker
  {
    private static readonly Stack<BackgroundWorker> IdleWorkers = new Stack<BackgroundWorker>();
    private readonly Thread _thread;
    private readonly AutoResetEvent _resetEvent;
    private Task _work;


    private BackgroundWorker()
    {
      _resetEvent = new AutoResetEvent(false);
      _thread = new Thread(WorkLoop);
      _thread.IsBackground = true;
      _thread.Start();
    }


    // ReSharper disable FunctionNeverReturns
    private void WorkLoop()
    {
      while (true)
      {
        _resetEvent.WaitOne();
        _work.DoWork();

        lock (IdleWorkers)
        {
          IdleWorkers.Push(this);
        }
      }
    }
    // ReSharper restore FunctionNeverReturns


    private void Start(Task work)
    {
      _work = work;
      _resetEvent.Set();
    }


    public static void StartWork(Task work)
    {
      BackgroundWorker worker = null;
      if (IdleWorkers.Count > 0)
      {
        lock (IdleWorkers)
        {
          if (IdleWorkers.Count > 0)
            worker = IdleWorkers.Pop();
        }
      }

      if (worker == null)
        worker = new BackgroundWorker();

      worker.Start(work);
    }
  }
}
#endif
