using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Com.Voobox.Project.Component
{
    public class FrameFreezerComponent
    {
        public async UniTask Freeze(float duration, float scale)
        {
            var originalScale = Time.timeScale;
            Time.timeScale = scale;
            await UniTask.Delay(TimeSpan.FromSeconds(duration), ignoreTimeScale: true);
            Time.timeScale = originalScale;
        }
    }
}