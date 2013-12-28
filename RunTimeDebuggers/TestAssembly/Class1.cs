using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace TestAssembly
{
    public class AClass
    {
        public AClass()
        {
            TestEventWiring();

        }
        internal class HitInfo
        {
            public enum eHitType
            {
                //kColumnHeader = 0x0001,
                //kColumnHeaderResize = 0x0002,
                kColumnHeader,
                kColumnHeaderResize,
            }

            public TestAssembly.AClass.HitInfo.eHitType HitType;
           
        }

        class IYamNested : List<AClass>
        {
            public int Nested { get; set; }

            
        }

        public unsafe static char* DefaultTest()
        {
           
                return default(char*);
           
        }

        public event EventHandler SomeEvent;

        public void TestEventWiring()
        {
            SomeEvent += new EventHandler(AClass_SomeEvent);
        }

        void AClass_SomeEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public unsafe byte ReturnNativeInt(Int32 test)
        {
            return (byte)test;
        }

        public static object OP_Add(float a, Int32 b)
        {
            return a + b;
        }
        //public class C<T>
        //{
        //    public class D : C<T>
        //    {

        //    }
        //}

        //struct ValA
        //{
        //    public ValB ValB { get; set; }
        //}

        //struct ValB
        //{
        //    public ValA ValA { get; set; }
        //}

        public class A : IList<AClass>
        {
            public virtual void AMethod()
            {
                Console.WriteLine("A!");
            }

            public int IndexOf(AClass item)
            {
                throw new NotImplementedException();
            }

            public void Insert(int index, AClass item)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public AClass this[int index]
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void Add(AClass item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(AClass item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(AClass[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(AClass item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<AClass> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public class B : A
        {
            public override void AMethod()
            {
                base.AMethod();
                Console.Write("B!");
            }


            public object GenericCopyFailWithToArray()
            {
                IEnumerable<A> list = new List<A>();
                return list.ToArray<A>();
            }
        }

        private delegate bool EnumWindowsCallBackDelegate(IntPtr hwnd, IntPtr lParam);
        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsCallBackDelegate callback, IntPtr lParam);

        [DllImport("user32.Dll")]
        private static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);

        private static bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam)
        {
            ((List<IntPtr>)((GCHandle)lParam).Target).Add(hwnd);
            return true;
        }

        private int afield;

        public int AProperty { get; set; }

        public void AMethod()
        {
            afield = 5;
            AProperty = 10;
        }

        public void AMethodWithLocals()
        {
            int a = 0;
            int b = 1;

            afield = a + b;
        }

        public void AMethodWithBranch()
        {
            int a = 0;
            int b = 1;

            if (a == 0)
                afield = b;
            else
                afield = a;

        }

        public string ATestString()
        {
            return DecryptString("uftu", 1);
        }

        public static string EncryptString(string str, int shift)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                output.Append((char)(((int)str[i]) + shift));
            }
            return output.ToString();
        }

        public static string DecryptString(string str, int shift)
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                output.Append((char)(((int)str[i]) - shift));
            }
            return output.ToString() ;
        }

        public void AGenericMethod<T>()
        {
            int a = typeof(T).MetadataToken;
        }

        public void AGenericMethodWithParameters<T>(List<T> obj, string test)
        {
            int a = typeof(T).MetadataToken;
        }

        public void AMethodWithTryCatch()
        {
            try
            {
                int x = 5;
                int y = 0;

                int result = x / y;
            }
            catch (Exception)
            {
                string test = "uhoh";
                DecryptString(test, 1);
            }
            finally
            {
                string final = "finally";
                DecryptString(final, 1);
            }
            //try
            //{
            //    try
            //    {
            //        throw new ArgumentException("Test");
            //    }
            //    catch (ArgumentException ex)
            //    {
            //        throw;
            //    }
            //    catch (InsufficientMemoryException ex)
            //    {
            //        afield = 77;

            //    }
            //}
            //catch (Exception)
            //{
            //    afield = 0;
            //    // nothing to worry about
            //}
            //finally
            //{
            //    AProperty = 10;
            //}


            afield = 5;
        }
    }


}
