using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UI.Common.ShortCuts
{
    public class DoubleShortCut
    {
        readonly Action _execute;
        readonly ICommand _cmd;
        readonly Key _keyFirst;
        readonly Key _keySecond;
        readonly ModifierKeys _modifier;
        readonly bool _needsAgreement;
        
        public DoubleShortCut()
        { }

        public DoubleShortCut( Action execute, Key keyfirst, Key keysecond, ModifierKeys modifier, bool needsAgreement = false )
        {
            _execute = execute;
            _cmd = null;
            _keyFirst = keyfirst;
            _keySecond = keysecond;
            _modifier = modifier;
            _needsAgreement = needsAgreement;
        }

        public DoubleShortCut( ICommand cmd, Key keyfirst, Key keysecond, ModifierKeys modifier, bool needsAgreement = false )
        {
            _execute = null;
            _cmd = cmd;
            _keyFirst = keyfirst;
            _keySecond = keysecond;
            _modifier = modifier;
            _needsAgreement = needsAgreement;
        }
        
        public bool CheckAndRunIt(ModifierKeys modifier, Key kfirst, Key ksecond, bool agreementOpen)
        {
            if (_needsAgreement)
            {
                if (!agreementOpen)
                {
                    return false;
                }
            }

            if (modifier == _modifier && _keyFirst == kfirst && _keySecond == ksecond)
            {
                if (_cmd == null)
                {
                    _execute();
                    return true;
                }
                else
                {
                    _cmd.Execute( null );
                    return true;
                }
            }
            return false;
        }

    }
}
