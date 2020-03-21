﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UlalaBatch.Infrastructure
{
    public class TaskBuilder
    {
        public static Task Build(Action action)
        {
            return Task.Run(action);
        }
        public static Task Build(Func<object, Task> func, out CancellationTokenSource tokenSource)
        {
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            return Task.Run(() =>
            {
                func(token);
            }, tokenSource.Token);
        }

        public static Task BuildAndDelay(Action action, int millisecondsDelay)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(millisecondsDelay);
                action();
            });
        }
    }
}
