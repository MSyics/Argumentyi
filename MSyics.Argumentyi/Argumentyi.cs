/****************************************************************
© 2017 MSyics
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
****************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace MSyics.Argumentyi
{
    /// <summary>
    /// 引数を指定した型に変換する機能を提供します。
    /// </summary>
    /// <typeparam name="T">変換する型</typeparam>
    public sealed class ArgumentParser<T>
    {
        /// <summary>
        /// ArgumentParser オブジェクトを生成します。
        /// </summary>
        /// <param name="setting">引数設定を行うためのデリゲート</param>
        /// <returns></returns>
        public static ArgumentParser<T> Create(Action<IArgumentSettingBuilder<T>> setting)
        {
            var builder = new ArgumentSettingBuilder<T>();
            setting(builder);
            return new ArgumentParser<T>() { Builder = builder };
        }

        /// <summary>
        /// ArgumentParser クラスのインスタンスを初期化します。
        /// </summary>
        private ArgumentParser()
        {
        }

        /// <summary>
        /// 引数から指定された型に変換します。
        /// </summary>
        public T Parse(string[] args)
        {
            var obj = Activator.CreateInstance<T>();

            var defaults = Builder.ArgumentSettings.Where(x => x.Pattern == ArgumentPattern.Default).GetEnumerator();
            var options = Builder.ArgumentSettings.Where(x => x.Pattern != ArgumentPattern.Default);

            for (int i = 0; i < args.Length; i++)
            {
                var option = options.FirstOrDefault(x =>
                {
                    if (IgnoreCase)
                    {
                        return x.Name.ToUpper() == args[i].ToUpper();
                    }
                    else
                    {
                        return x.Name == args[i];
                    }
                });
                if (option == null)
                {
                    if (defaults.MoveNext())
                    {
                        defaults.Current.SetValue(obj, args[i]);
                    }
                }
                else
                {
                    if (option.Pattern == ArgumentPattern.OptionWith)
                    {
                        var value = args.Skip(i + 1)
                                        .TakeWhile(x => !options.Any(y => y.Name == x))
                                        .FirstOrDefault();

                        if (string.IsNullOrEmpty(value))
                        {
                            // 値が設定されていない場合は例外を投げる。
                            throw new ArgumentException(option.Name);
                        }

                        option.SetValue(obj, value);
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            ++i;
                        }
                    }
                    else
                    {
                        option.SetValue(obj, null);
                    }
                }
            }

            if (defaults.MoveNext())
            {
                // 既定値が設定されていない場合は例外を投げる。
                throw new ArgumentException(defaults.Current.Name);
            }

            return obj;
        }

        /// <summary>
        /// 引数から指定された型への変換を試みます。
        /// </summary>
        public bool TryParse(string[] args, out T obj)
        {
            try
            {
                obj = Parse(args);
                return true;
            }
            catch (Exception)
            {
                obj = default(T);
                return false;
            }
        }

        /// <summary>
        /// 引数のビルダーを取得または設定します。
        /// </summary>
        internal ArgumentSettingBuilder<T> Builder { get; set; }

        /// <summary>
        /// 引数とオプション文字列を比較する場合に、大文字と小文字を区別するかどうかを示す値を取得または設定します。
        /// </summary>
        public bool IgnoreCase { get; set; }
    }

    /// <summary>
    /// 引数を表します。
    /// </summary>
    internal sealed class ArgumentSetting
    {
        /// <summary>
        /// 名前を取得します。
        /// </summary>
        public string Name { get; internal set; }

        internal Action<object, string> SetValue { get; set; }
        internal ArgumentPattern Pattern { get; set; }
    }

    /// <summary>
    /// 引数のパターンを示す値
    /// </summary>
    internal enum ArgumentPattern
    {
        /// <summary>
        /// 既定値
        /// </summary>
        Default,

        /// <summary>
        /// 引数
        /// </summary>
        Option,

        /// <summary>
        /// 指定する値がある引数
        /// </summary>
        OptionWith,
    }

    /// <summary>
    /// 引数設定を構築する機能を提供します。
    /// </summary>
    public interface IArgumentSettingBuilder<T>
    {
        /// <summary>
        /// オプションの引数設定を構築します。
        /// </summary>
        /// <param name="name">オプションの名前を設定します。</param>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        /// <param name="value">プロパティに設定する値を返すデリゲートを設定します。</param>
        IArgumentSettingBuilder<T> Option<TValue>(string name, Expression<Func<T, TValue>> property, Func<TValue> value);

        /// <summary>
        /// オプションの引数設定を構築します。
        /// </summary>
        /// <param name="name">オプションの名前を設定します。</param>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        /// <param name="value">引数の文字列からプロパティに設定する値を返すデリゲートを設定します。</param>
        IArgumentSettingBuilder<T> Option<TValue>(string name, Expression<Func<T, TValue>> property, Func<string, TValue> value);

        /// <summary>
        /// オプションの引数設定を構築します。
        /// </summary>
        /// <param name="name">オプションの名前を設定します。</param>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        IArgumentSettingBuilder<T> Option(string name, Expression<Func<T, String>> property);

        /// <summary>
        /// 既定の引数設定を構築します。
        /// </summary>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        /// <param name="value">引数の文字列からプロパティに設定する値を返すデリゲートを設定します。</param>
        IArgumentSettingBuilder<T> Default<TValue>(Expression<Func<T, TValue>> property, Func<string, TValue> value);

        /// <summary>
        /// 既定の引数設定を構築します。
        /// </summary>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        IArgumentSettingBuilder<T> Default(Expression<Func<T, String>> property);
    }

    /// <summary>
    /// 引数設定を構築する機能を提供します。
    /// </summary>
    internal sealed class ArgumentSettingBuilder<T> : IArgumentSettingBuilder<T>
    {
        /// <summary>
        /// オプションの引数設定を構築します。
        /// </summary>
        /// <param name="name">オプションの名前を設定します。</param>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        /// <param name="value">プロパティに設定する値を返すデリゲートを設定します。</param>
        public IArgumentSettingBuilder<T> Option<TValue>(string name, Expression<Func<T, TValue>> property, Func<TValue> value)
        {
            ArgumentSettings.Add(new ArgumentSetting()
            {
                Name = name,
                SetValue = (x, y) => ((PropertyInfo)((MemberExpression)property.Body).Member).SetValue(x, value(), null),
                Pattern = ArgumentPattern.Option,
            });
            return this;
        }

        /// <summary>
        /// オプションの引数設定を構築します。
        /// </summary>
        /// <param name="name">オプションの名前を設定します。</param>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        /// <param name="value">引数の文字列からプロパティに設定する値を返すデリゲートを設定します。</param>
        public IArgumentSettingBuilder<T> Option<TValue>(string name, Expression<Func<T, TValue>> property, Func<string, TValue> value)
        {
            ArgumentSettings.Add(new ArgumentSetting()
            {
                Name = name,
                SetValue = (x, y) => ((PropertyInfo)((MemberExpression)property.Body).Member).SetValue(x, value(y), null),
                Pattern = ArgumentPattern.OptionWith,
            });
            return this;
        }

        /// <summary>
        /// オプションの引数設定を構築します。
        /// </summary>
        /// <param name="name">オプションの名前を設定します。</param>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        public IArgumentSettingBuilder<T> Option(string name, Expression<Func<T, String>> property)
        {
            return Option(name, property, x => x);
        }

        /// <summary>
        /// 既定の引数設定を構築します。
        /// </summary>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        /// <param name="value">引数の文字列からプロパティに設定する値を返すデリゲートを設定します。</param>
        public IArgumentSettingBuilder<T> Default<TValue>(Expression<Func<T, TValue>> property, Func<string, TValue> value)
        {
            ArgumentSettings.Add(new ArgumentSetting()
            {
                Name = ((MemberExpression)property.Body).Member.Name,
                SetValue = (x, y) => ((PropertyInfo)((MemberExpression)property.Body).Member).SetValue(x, value(y), null),
                Pattern = ArgumentPattern.Default,
            });
            return this;
        }

        /// <summary>
        /// 既定の引数設定を構築します。
        /// </summary>
        /// <param name="member">値を設定するプロパティを設定します。</param>
        public IArgumentSettingBuilder<T> Default(Expression<Func<T, String>> property)
        {
            return Default(property, x => x);
        }

        /// <summary>
        /// 引数設定の一覧を取得します。
        /// </summary>
        public List<ArgumentSetting> ArgumentSettings { get; } = new List<ArgumentSetting>();
    }
}
