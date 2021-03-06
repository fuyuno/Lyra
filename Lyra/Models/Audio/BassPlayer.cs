﻿using System;
using System.IO;
using System.Reactive.Linq;

using Lyra.Events;

using Un4seen.Bass;

namespace Lyra.Models.Audio
{
    /// <summary>
    /// BASS for .NET
    /// 参考：http://akabeko.me/blog/2010/01/c-%E3%81%A7%E9%9F%B3%E6%A5%BD%E5%86%8D%E7%94%9F-3/
    /// </summary>
    public class BassPlayer : IAudioPlayer
    {
        private int _handle;
        private float _volume;
        private bool _flag;
        private Track _track;

        public event PlayingStreamFinishedEvent OnPlayingStreamFinished;

        public BassPlayer()
        {
            // SplashScreen 出るとかいうクソ仕様
            BassNet.Registration(LyraApp.BassNetMailAddress, LyraApp.BassNetRegistrationKey);

            // 初期化
            if (!Bass.LoadMe(LyraApp.BassNetModuleDir))
                throw new Exception("Bass.NET の初期化に失敗しました。");

            // デフォを使う
            if (!Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                throw new Exception("デバイスの初期化に失敗しました。");

            // Add-on 周り
            Bass.BASS_PluginLoadDirectory(LyraApp.BassNetModuleDir);

            // 終了通知イベント周り
            this._flag = true;

            // Stream 再生が終了した時だけ Finish 通知を送信。
            Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(20))
                .Select(_ => this.PlayState)                        // this.PlayState を 20ms ごとに取得
                .DistinctUntilChanged()                             // 値が変わったら次へ
                .Where(_ => !this._flag)                            // かつ Stop() での停止ではない場合は通知。
                .Where(_ => this.PlayState == PlayState.Stopped)    // this.PlayState が Stopped (停止状態)の場合で、
                .Repeat()
                .Subscribe(_ => OnPlayingStreamFinished?.Invoke(this, new PlayingStreamFinishedEventArgs()));
        }

        /// <summary>
        /// 音声ファイルを再生します。
        /// ポーズから再開する場合は、Trackはnullもしくは同じものにします。
        /// </summary>
        /// <param name="track">再生するトラック</param>
        public void Play(Track track)
        {
            if (this.PlayState == PlayState.Playing)
            {
                if (this._track.Id == track.Id)
                    return;
                this.Stop();
            }

            var path = track.Path;

            // handler
            if (path != null && this.PlayState != PlayState.Paused)
            {
                // File?
                if (File.Exists(path))
                    this._handle = Bass.BASS_StreamCreateFile(path, 0, 0, BASSFlag.BASS_DEFAULT);
                else if (path.StartsWith("https://") || path.StartsWith("http://") || path.StartsWith("ftp://"))
                    this._handle = Bass.BASS_StreamCreateURL(path, 0, BASSFlag.BASS_DEFAULT, null, IntPtr.Zero);

                if (this._handle == 0)
                    throw new Exception("ファイルを再生できません。");
            }

            Bass.BASS_ChannelPlay(this._handle, this.PlayState != PlayState.Paused);
            this._track = track;
            this._flag = false;
        }

        /// <summary>
        /// 現在再生中のファイルをポーズします。
        /// 再度再生する場合は、Playを呼び出してください。
        /// </summary>
        public void Pause()
        {
            if (this.PlayState != PlayState.Playing)
                return;

            Bass.BASS_ChannelPause(this._handle);
        }

        /// <summary>
        /// 現在再生中のファイルをストップします。
        /// </summary>
        public void Stop()
        {
            if (this.PlayState == PlayState.Stopped)
                return;

            Bass.BASS_ChannelStop(this._handle);
            this._flag = true;

            // 再生箇所を初期化
            Bass.BASS_ChannelSetPosition(this._handle, 0.0);

            // ファイルを開放
            Bass.BASS_StreamFree(this._handle);

            this._track = null;
            this._handle = 0;
        }

        /// <summary>
        /// 現在の再生状況を取得します。
        /// </summary>
        public PlayState PlayState
        {
            get
            {
                var state = Bass.BASS_ChannelIsActive(this._handle);
                // switch 使うまでもないような
                if (state == BASSActive.BASS_ACTIVE_PLAYING)
                    return PlayState.Playing;
                else if (state == BASSActive.BASS_ACTIVE_PAUSED)
                    return PlayState.Paused;
                else
                    return PlayState.Stopped;
            }
        }

        /// <summary>
        /// 現在のボリュームを取得/設定します。
        /// なお、音声が再生されていない場合は最後に設定した値が取得されます。
        /// </summary>
        public float Volume
        {
            get
            {
                var volume = this._volume;
                var b = Bass.BASS_ChannelGetAttribute(this._handle, BASSAttribute.BASS_ATTRIB_VOL, ref volume);
                if (!b)
                    return this._volume;
                return volume;
            }
            set
            {
                Bass.BASS_ChannelSetAttribute(this._handle, BASSAttribute.BASS_ATTRIB_VOL, value);
                this._volume = value;
            }
        }

        /// <summary>
        /// 現在再生中の時間を取得/設定します。
        /// </summary>
        public long CurrentTime
        {
            get
            {
                var pos = Bass.BASS_ChannelGetPosition(this._handle);
                return (long)(Bass.BASS_ChannelBytes2Seconds(this._handle, pos) * 1000);
            }
            set
            {
                var pos = Bass.BASS_ChannelSeconds2Bytes(this._handle, value / 1000f);
                Bass.BASS_ChannelSetPosition(this._handle, pos);
            }
        }

        public void Dispose()
        {
            if (this._handle != 0)
                this.Stop();

            Bass.BASS_PluginFree(0);
            Bass.FreeMe();
        }
    }
}