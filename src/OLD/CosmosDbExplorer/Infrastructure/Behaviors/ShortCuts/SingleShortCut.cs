using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace UI.Common.ShortCuts
{
    public class SingleShortCut
    {
        readonly Action _execute;
        readonly ICommand _cmd;
        readonly Key _key;
        readonly ModifierKeys _modifier;
        readonly bool _needsAgreement;

        public SingleShortCut()
        { }

        public SingleShortCut( Action execute, Key key, ModifierKeys modifier, bool needsAgreement = false )
        {
            _execute = execute;
            _cmd = null;
            _key = key;
            _modifier = modifier;
            _needsAgreement = needsAgreement;
        }

        public SingleShortCut( ICommand cmd, Key key, ModifierKeys modifier, bool needsAgreement = false )
        {
            _execute = null;
            _cmd = cmd;
            _key = key;
            _modifier = modifier;
            _needsAgreement = needsAgreement;
        }

        public bool CheckAndRunIt(ModifierKeys modifier, Key k, bool agreementOpen)
        {
            if (_needsAgreement)
            {
                if (!agreementOpen)
                {
                    return false;
                }
            }

            if (modifier == _modifier && _key == k)
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
