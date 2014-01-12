using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Permissions;


namespace TestForm
{
    [TestAttribute("TEST attribute")]
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [SecurityPermission(SecurityAction.Assert, Unrestricted = false, Execution = true, UnmanagedCode = true)]
    public partial class MainForm : Form, IFoo
    {

        private enum TestEnum
        {
            AnEnumValue,
            AnotherEnumValue,
            AndAnotherOne
        }

        [TestAttribute("TEST attribute on constructor")]
        public MainForm()
        {
            InitializeComponent();

            someStrings = new List<string>() { "A", "list", "of", "strings" };
            StaticProperty = 7707;

            DoSomethingWithEnum(TestEnum.AnEnumValue);

            TestOps();

            //Dictionary<Type, int> testDic = new Dictionary<Type, int>();
            //testDic.Add(typeof(GenericClass2<>), 0);
            //testDic.Add(typeof(GenericClass2<MainForm>), 0);
            //testDic.Add(typeof(GenericClass2<string>), 0);
            //testDic.Add(typeof(GenericClass2<GenericClass2<string>>), 0);

            try
            {
                MethodException();
            }
            catch (Exception ex)
            {
                var str = ex.StackTrace;
                Console.WriteLine(str);
            }
        }

        void DoSomethingWithEnum([Test("Test attribute on parameter")]TestEnum enm)
        {
            Text = ((int)enm).ToString();
        }

        //private class TestFilledIn : GenericClass2<string>
        //{

        //}

        //private class TestNestedGeneric<T> : GenericClass2<GenericClass3<T>>
        //    where T : struct
        //{

        //}

        private TestForm testfield1 = new TestForm();
        private static TestForm testfield2 = new TestForm();

        public string Method1()
        {
            return "Method 1 value (" + DateTime.Now.ToString("HH:mm:ss") + ")";
        }

        public string Method2<T>()
        {
            return "Method 2 value, type param=" + typeof(T).Name.ToString() + " (" + DateTime.Now.ToString("HH:mm") + ")";
        }

        public string Method2<K, V>()
        {
            return "Method 2 value, type param=" + typeof(K).Name.ToString() + "," + typeof(V).Name.ToString() + " (" + DateTime.Now.ToString("HH:mm") + ")";
        }

        public string Method3(string arg)
        {
            return "Method 4 value, args=" + arg + " (" + DateTime.Now.ToString("HH:mm") + ")";
        }

        public string Method4(string arg1, bool arg2)
        {
            return "Method 4 value, args=" + arg1 + "," + arg2 + " (" + DateTime.Now.ToString("HH:mm") + ")";
        }

        public string Method5<T>(T arg)
        {
            return "Method 5 value, type param=" + typeof(T).Name.ToString() + ", args=" + arg + " (" + DateTime.Now.ToString("HH:mm") + ")";
        }

        public void TestOps()
        {
            int resultAdd = TestAdd(2,3);
            int resultSubtract = TestSubtract(10, 5);
            int result;
            TestAddWithOutResult(2, 3, out result);
            if (result != resultAdd)
                throw new Exception("Out didn't work well");
        }

        private int TestAdd(int x, int y)
        {
            return x + y;
        }

        private int TestSubtract(int x, int y)
        {
            return x - y;
        }

        private void TestAddWithOutResult(int x, int y, out int result)
        {
            result = x + y;
        }

        public DateTime CurrentTime { get { return DateTime.Now; } }

        private DateTime PrivCurrentTime { get { return DateTime.Now; } }

        protected DateTime ProtCurrentTime { get { return DateTime.Now; } }

        internal DateTime IntCurrentTime { get { return DateTime.Now; } }

        public DateTime MethodException()
        {
            if (DateTime.Now.Second % 2 == 0)
                throw new ArgumentException("Method exception!");
            else
                return DateTime.Now;

        }

        private List<string> someStrings;

        private string iAmNull = null;


        // test with this.Orly(new List<Dictionary<string, KeyValuePair<int, List<bool>>>>(), someStrings[0]);
        public List<Dictionary<K, KeyValuePair<T1, List<T2>>>> Orly<K, T1, T2>(List<Dictionary<K, KeyValuePair<T1, List<T2>>>> parameter, string foo)
        {

            return null;

        }


        private static string staticField = "I am a static member!";

        public static int StaticProperty { get; set; }

        public string this[int x]
        {
            get
            {
                return x.ToString();
            }
            set
            {

            }
        }

        public string this[int x, int y]
        {
            get
            {
                return x + "," + y;
            }
            set
            {

            }
        }

        public string this[string foo, int y]
        {
            get
            {
                return foo + "," + y;
            }
            set
            {

            }
        }



        private TestForm.InnerClass TestInner = new TestForm.InnerClass() { Test = 5 };

        public override bool Equals(object obj)
        {
            Console.WriteLine("meh");

            return base.Equals(obj);
        }
        private void btnTest_Click(object sender, EventArgs e)
        {

            //MethodBodyReader reader = new MethodBodyReader(typeof(TestForm).GetMethod("TestIfAnd"));
            //CSharpILParser parser = new CSharpILParser(reader.Method, reader.Instructions);
            //string str = parser.Parse();

            //MessageBox.Show(str);
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Writing some " + Environment.NewLine + "console text ...");
            Trace.WriteLine("Writing some " + Environment.NewLine + " trace text ...");

            Debug.WriteLine("Writing some " + Environment.NewLine + " debug text ...");

        }

        private void btnValidateSerial_Click(object sender, EventArgs e)
        {
            if (!ライセンスマネージャ.シリアル検証(textBox1.Text))
            {
                MessageBox.Show("The input is not a valid serial, non commercial license will be used");
            }
            else
                MessageBox.Show("Serial is valid, thanks for purchasing!");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!ライセンスマネージャ.A || string.IsNullOrEmpty(ライセンスマネージャ.a))
            {
                MessageBox.Show("You're using a non commercial license, this function is disabled");
                return;
            }
            else
            {
                MessageBox.Show("I'm doing something right now!");
            }
        }


        int IFoo.MyFoo
        {
            get;
            set;
        }
    }

    public static class ライセンスマネージャ
    {

        private static bool b;

        public static string a { get; private set; }

        public static bool A
        {
            get { return ライセンスマネージャ.b; }
            set { ライセンスマネージャ.b = value; }
        }

        public static bool シリアル検証(string s)
        {
            if (GetMD5Hash(s) == "acbd18db4cc2f85cedef654fccc4a4d8")
            {
                b = true;
                a = s;
                return true;
            }
            else
            {
                b = false;
                a = "";
                return false;
            }
        }

        public static string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }
    }

    //public class GenericClass<T> where T : TestForm
    //{

    //    public static T TestGenericClass(T test)
    //    {
    //        return test;
    //    }
    //}

    //public class GenericClass2<T> where T : class
    //{
    //    public static T TestGenericClass(T test)
    //    {
    //        return test;
    //    }
    //}

    //public class GenericClass3<T> where T : struct
    //{
    //    public static T TestGenericClass(T test)
    //    {
    //        return test;
    //    }
    //}

    //public class GenericClass4<T> where T : new()
    //{
    //    public static T TestGenericClass(T test)
    //    {
    //        return test;
    //    }
    //}


    public class TestForm
    {
        public class InnerClass
        {
            public int Test { get; set; }
        }

        public string TestIf()
        {
            bool a = true;
            if (a)
                return "true";
            else
                return "false;";
        }

        public string TestIfAnd()
        {
            bool a = true;
            bool b = false;

            string meh = "foo";

            if (a && b)
            {
                meh = "truepart";
            }
            else
            {
                meh = "falsepart";
            }
            return meh;
        }

        public string TestIfOr()
        {
            bool a = true;
            bool b = false;
            if (a || b)
                return "true";
            else
                return "false;";
        }

        public string TestIfComplex()
        {
            bool a = true;
            bool b = false;
            bool c = false;
            if (a && (b || c))
                return "true";
            else
                return "false;";
        }

        public string TestWhile()
        {
            int i = 0;
            string str = "";

            while (i < 77)
            {
                str = "whiling";
                i++;
            }
            return str + "afterwhile";
        }

        public string TestFor()
        {
            string s;
            for (int i = 0; i < 10; i++)
            {
                s = "for";
            }
            return "afterfor";
        }

    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class TestAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public TestAttribute(string positionalString)
        {
            this.positionalString = positionalString;
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }

}
