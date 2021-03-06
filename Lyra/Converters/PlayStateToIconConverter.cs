﻿using System;
using System.Globalization;
using System.Windows.Data;

using Lyra.Models;
using Lyra.Models.Audio;

namespace Lyra.Converters
{
    /// <summary>
    /// <see cref="Lyra.Models.Audio.PlayState"/> を 定義されたアイコンに変換します。
    /// </summary>
    public class PlayStateToIconConverter : IValueConverter
    {
        /// <summary>
        /// 値を変換します。
        /// </summary>
        /// <returns>
        /// 変換された値。メソッドが null を返す場合は、有効な null 値が使用されています。
        /// </returns>
        /// <param name="value">バインディング ソースによって生成された値。</param><param name="targetType">バインディング ターゲット プロパティの型。</param><param name="parameter">使用するコンバーター パラメーター。</param><param name="culture">コンバーターで使用するカルチャ。</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is PlayState))
                return null;

            IconType type;
            if (!(parameter is IconType))
                type = IconType.Player;
            else
                type = (IconType)parameter;

            var state = (PlayState)value;
            if (type == IconType.Player)
            {
                if (state == PlayState.Playing)
                    return LyraApp.ButtonPause;
                else
                    return LyraApp.ButtonPlay;
            }
            else
            {
                if (state == PlayState.Paused)
                    return LyraApp.ButtonPause;
                else if (state == PlayState.Playing)
                    return LyraApp.ButtonPlay;
                else
                    return null;
            }
        }

        /// <summary>
        /// 値を変換します。
        /// </summary>
        /// <returns>
        /// 変換された値。メソッドが null を返す場合は、有効な null 値が使用されています。
        /// </returns>
        /// <param name="value">バインディング ターゲットによって生成される値。</param><param name="targetType">変換後の型。</param><param name="parameter">使用するコンバーター パラメーター。</param><param name="culture">コンバーターで使用するカルチャ。</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}