using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Drawing;
using RunTimeDebuggers.Properties;

namespace RunTimeDebuggers.Helpers
{

    internal enum IconEnum : int
    {
        Bullet, 

        PrivateField,
        ProtectedField,
        PublicField,
        InternalField,
        ProtectedInternalField,

        StaticPrivateField,
        StaticProtectedField,
        StaticPublicField,
        StaticInternalField,
        StaticProtectedInternalField,

        PrivateProperty,
        ProtectedProperty,
        PublicProperty,
        InternalProperty,
        ProtectedInternalProperty,

        StaticPrivateProperty,
        StaticProtectedProperty,
        StaticPublicProperty,
        StaticInternalProperty,
        StaticProtectedInternalProperty,

        EvaluatedStatement,

        PrivateMethod,
        ProtectedMethod,
        PublicMethod,
        InternalMethod,
        ProtectedInternalMethod,

        StaticPrivateMethod,
        StaticProtectedMethod,
        StaticPublicMethod,
        StaticInternalMethod,
        StaticProtectedInternalMethod,

        ExtensionMethod,

        PrivateConstructor,
        ProtectedConstructor,
        PublicConstructor,
        InternalConstructor,
        ProtectedInternalConstructor,

        StaticPrivateConstructor,
        StaticProtectedConstructor,
        StaticPublicConstructor,
        StaticInternalConstructor,
        StaticProtectedInternalConstructor,


        Assembly,
        Namespace,

        PrivateClass,
        ProtectedClass,
        PublicClass,
        InternalClass,
        ProtectedInternalClass,


        StaticPrivateClass,
        StaticProtectedClass,
        StaticPublicClass,
        StaticInternalClass,
        StaticProtectedInternalClass,

        PrivateStruct,
        ProtectedStruct,
        PublicStruct,
        InternalStruct,
        ProtectedInternalStruct,

        PrivateInterface,
        ProtectedInterface,
        PublicInterface,
        InternalInterface,
        ProtectedInternalInterface,

        PrivateEnum,
        ProtectedEnum,
        PublicEnum,
        InternalEnum,
        ProtectedInternalEnum,

        PrivateEvent,
        ProtectedEvent,
        PublicEvent,
        InternalEvent,
        ProtectedInternalEvent,

        StaticPrivateEvent,
        StaticProtectedEvent,
        StaticPublicEvent,
        StaticInternalEvent,
        StaticProtectedInternalEvent,

        ReferenceFolderClosed,
        ReferenceFolderOpen,
        BaseTypes,

        ResourcesFile,
        Resource

    }

    static class IconHelper
    {

        public static IEnumerable<Image> GetIcons()
        {
            yield return Resources.bullet;

            yield return (DrawOverlay(Resources.Field, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.Field, OverlayEnum.Private, true));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.Protected, true ));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.Public, true));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.Internal, true));
            yield return (DrawOverlay(Resources.Field, OverlayEnum.InternalProtected, true));

            yield return (DrawOverlay(Resources.Property, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.Property, OverlayEnum.Private, true));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.Protected, true));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.Public, true));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.Internal, true));
            yield return (DrawOverlay(Resources.Property, OverlayEnum.InternalProtected, true));

            //yield return (Resources.protfield);
            //yield return (Resources.pubfield);
            //yield return (Resources.intfield);
            //yield return (Resources.privproperty);
            //yield return (Resources.protproperty);
            //yield return (Resources.pubproperty);
            //yield return (Resources.intproperty);
            yield return (Resources.eval);

            yield return (DrawOverlay(Resources.Method, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.Method, OverlayEnum.Private, true));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.Protected, true));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.Public, true));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.Internal, true));
            yield return (DrawOverlay(Resources.Method, OverlayEnum.InternalProtected, true));


            //yield return (Resources.privmethod);
            //yield return (Resources.protmethod);
            //yield return (Resources.pubmethod);
            //yield return (Resources.intmethod);
            yield return (Resources.extmethod);


            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Internal, false ));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.InternalProtected, false));


            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Private, true));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Protected, true));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Public, true));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.Internal, true));
            yield return (DrawOverlay(Resources.Constructor, OverlayEnum.InternalProtected ,true));

            //yield return (Resources.Constructor);

            yield return Resources.Assembly;
            yield return Resources.NameSpace;

            //yield return Resources.Class;
            yield return (DrawOverlay(Resources.Class, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Class, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Class, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Class, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Class, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.StaticClass, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.StaticClass, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.StaticClass, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.StaticClass, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.StaticClass, OverlayEnum.InternalProtected, false));


            //yield return Resources.Struct;
            yield return (DrawOverlay(Resources.Struct, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Struct, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Struct, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Struct, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Struct, OverlayEnum.InternalProtected, false));

            //yield return Resources.Interface;
            yield return (DrawOverlay(Resources.Interface, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Interface, OverlayEnum.Protected, false ));
            yield return (DrawOverlay(Resources.Interface, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Interface, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Interface, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.Enum, OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Enum, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Enum, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Enum, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Enum, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.Event , OverlayEnum.Private, false));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.Protected, false));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.Public, false));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.Internal, false));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.InternalProtected, false));

            yield return (DrawOverlay(Resources.Event, OverlayEnum.Private, true));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.Protected, true));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.Public, true));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.Internal, true));
            yield return (DrawOverlay(Resources.Event, OverlayEnum.InternalProtected, true));


            yield return Resources.ReferenceFolder_Closed;
            yield return Resources.ReferenceFolder_Open;

            yield return Resources.basetype;

            yield return Resources.ResourceResourcesFile;
            yield return Resources.Resource;
        }

        private enum OverlayEnum
        {
            Private,
            Protected,
            Internal,
            InternalProtected,
            Public
        }

        private static Image DrawOverlay(Image img, OverlayEnum overlay, bool drawStatic)
        {
            Bitmap bmp = new Bitmap(img);
            img.Dispose();
            using (var g = Graphics.FromImage(bmp))
            {
                if (overlay == OverlayEnum.Private)
                    g.DrawImage(Resources.OverlayPrivate, new Point(0, 0));
                else if (overlay == OverlayEnum.Internal)
                    g.DrawImage(Resources.OverlayInternal, new Point(0, 0));
                else if (overlay == OverlayEnum.InternalProtected)
                    g.DrawImage(Resources.OverlayProtectedInternal, new Point(0, 0));
                else if (overlay == OverlayEnum.Protected)
                    g.DrawImage(Resources.OverlayProtected, new Point(0, 0));

                if(drawStatic)
                g.DrawImage(Resources.OverlayStatic, new Point(0, 0));
            }
            return bmp;
        }

        public static int GetIcon(this Type type)
        {

            if (type.IsInterface)
            {
                if (type.IsNested)
                {
                    if (type.IsNestedPrivate)
                        return (int)IconEnum.PrivateInterface;
                    else if (type.IsNestedFamily)
                        return (int)IconEnum.ProtectedInterface;
                    else if (type.IsNestedPublic)
                        return (int)IconEnum.PublicInterface;
                    else if (type.IsNestedAssembly)
                        return (int)IconEnum.InternalInterface;
                    else if (type.IsNestedFamANDAssem)
                        return (int)IconEnum.ProtectedInternalInterface;
                }
                else
                {
                    if (type.IsPublic)
                        return (int)IconEnum.PublicInterface;
                    else if (type.IsNotPublic)
                        return (int)IconEnum.InternalInterface;
                }
            }
            else if (type.IsEnum)
            {
                if (type.IsNested)
                {
                    if (type.IsNestedPrivate)
                        return (int)IconEnum.PrivateEnum;
                    else if (type.IsNestedFamily)
                        return (int)IconEnum.ProtectedEnum;
                    else if (type.IsNestedPublic)
                        return (int)IconEnum.PublicEnum;
                    else if (type.IsNestedAssembly)
                        return (int)IconEnum.InternalEnum;
                    else if (type.IsNestedFamANDAssem)
                        return (int)IconEnum.ProtectedInternalEnum;
                }
                else
                {
                    if (type.IsPublic)
                        return (int)IconEnum.PublicEnum;
                    else if (type.IsNotPublic)
                        return (int)IconEnum.InternalEnum;
                }
            }
            else if (type.IsClass)
            {
                if (type.IsNested)
                {
                    if (type.IsNestedPrivate)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticPrivateClass : (int)IconEnum.PrivateClass;
                    else if (type.IsNestedFamily)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticProtectedClass : (int)IconEnum.ProtectedClass;
                    else if (type.IsNestedPublic)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticPublicClass : (int)IconEnum.PublicClass;
                    else if (type.IsNestedAssembly)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticInternalClass : (int)IconEnum.InternalClass;
                    else if (type.IsNestedFamANDAssem)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticProtectedInternalClass : (int)IconEnum.ProtectedInternalClass;
                }
                else
                {
                    if (type.IsPublic)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticPublicClass : (int)IconEnum.PublicClass;
                    else if (type.IsNotPublic)
                        return type.IsAbstract && type.IsSealed ? (int)IconEnum.StaticInternalClass : (int)IconEnum.InternalClass;
                }
            }
            else if (type.IsValueType)
            {
                if (type.IsNested)
                {
                    if (type.IsNestedPrivate)
                        return (int)IconEnum.PrivateStruct;
                    else if (type.IsNestedFamily)
                        return (int)IconEnum.ProtectedStruct;
                    else if (type.IsNestedPublic)
                        return (int)IconEnum.PublicStruct;
                    else if (type.IsNestedAssembly)
                        return (int)IconEnum.InternalStruct;
                    else if (type.IsNestedFamANDAssem)
                        return (int)IconEnum.ProtectedInternalStruct;
                }
                else
                {
                    if (type.IsPublic)
                        return (int)IconEnum.PublicStruct;
                    else if (type.IsNotPublic)
                        return (int)IconEnum.InternalStruct;
                }
            }
            return -1;
        }


        public static int GetIcon(this MemberInfo member)
        {
            if (member is FieldInfo)
            {
                FieldInfo f = (FieldInfo)member;
                if (!f.IsStatic)
                {
                    if (f.IsPrivate)
                        return (int)IconEnum.PrivateField;
                    else if (f.IsFamily)
                        return (int)IconEnum.ProtectedField;
                    else if (f.IsPublic)
                        return (int)IconEnum.PublicField;
                    else if (f.IsAssembly)
                        return (int)IconEnum.InternalField;
                    else if (f.IsFamilyAndAssembly)
                        return (int)IconEnum.ProtectedInternalField;
                }
                else
                    if (f.IsPrivate)
                        return (int)IconEnum.StaticPrivateField;
                    else if (f.IsFamily)
                        return (int)IconEnum.StaticProtectedField;
                    else if (f.IsPublic)     
                        return (int)IconEnum.StaticPublicField;
                    else if (f.IsAssembly)   
                        return (int)IconEnum.StaticInternalField;
                    else if (f.IsFamilyAndAssembly)
                        return (int)IconEnum.StaticProtectedInternalField;

            }
            else if (member is PropertyInfo)
            {
                PropertyInfo prop = (PropertyInfo)member;
                var getMethod = prop.GetGetMethod(true);
                if (getMethod != null)
                {
                    if (!getMethod.IsStatic)
                    {
                        if (getMethod.IsPrivate)
                            return (int)IconEnum.PrivateProperty;
                        else if (getMethod.IsFamily)
                            return (int)IconEnum.ProtectedProperty;
                        else if (getMethod.IsPublic)
                            return (int)IconEnum.PublicProperty;
                        else if (getMethod.IsAssembly)
                            return (int)IconEnum.InternalProperty;
                        else if (getMethod.IsFamilyAndAssembly)
                            return (int)IconEnum.ProtectedInternalProperty;
                    }
                    else
                    {
                        if (getMethod.IsPrivate)
                            return (int)IconEnum.StaticPrivateProperty;
                        else if (getMethod.IsFamily)
                            return (int)IconEnum.StaticProtectedProperty;
                        else if (getMethod.IsPublic)
                            return (int)IconEnum.StaticPublicProperty;
                        else if (getMethod.IsAssembly)
                            return (int)IconEnum.StaticInternalProperty;
                        else if (getMethod.IsFamilyAndAssembly)
                            return (int)IconEnum.StaticProtectedInternalProperty;
                    }
                }
                //else
                //    ImageId = (int)IconEnum.PublicProperty;
            }
            else if (member is EventInfo)
            {
                EventInfo ev = (EventInfo)member;
                var addMethod = ev.GetAddMethod(true);
                if (addMethod != null)
                {
                    if (!addMethod.IsStatic)
                    {
                        if (addMethod.IsPrivate)
                            return (int)IconEnum.PrivateEvent;
                        else if (addMethod.IsFamily)
                            return (int)IconEnum.ProtectedEvent;
                        else if (addMethod.IsPublic)
                            return (int)IconEnum.PublicEvent;
                        else if (addMethod.IsAssembly)
                            return (int)IconEnum.InternalEvent;
                        else if (addMethod.IsFamilyAndAssembly)
                            return (int)IconEnum.ProtectedInternalEvent;
                    }
                    else
                    {
                        if (addMethod.IsPrivate)
                            return (int)IconEnum.StaticPrivateEvent;
                        else if (addMethod.IsFamily)
                            return (int)IconEnum.StaticProtectedEvent;
                        else if (addMethod.IsPublic)
                            return (int)IconEnum.StaticPublicEvent;
                        else if (addMethod.IsAssembly)
                            return (int)IconEnum.StaticInternalEvent;
                        else if (addMethod.IsFamilyAndAssembly)
                            return (int)IconEnum.StaticProtectedInternalEvent;
                    }
                }
                //else
                //    ImageId = (int)IconEnum.PublicProperty;
            }
            else if (member is MethodInfo)
            {
                if (member.IsDefined(typeof(ExtensionAttribute), false))
                {
                    return (int)IconEnum.ExtensionMethod;
                }
                else
                {
                    MethodInfo method = (MethodInfo)member;

                    if (!method.IsStatic)
                    {
                        if (method.IsPrivate)
                            return (int)IconEnum.PrivateMethod;
                        else if (method.IsFamily)
                            return (int)IconEnum.ProtectedMethod;
                        else if (method.IsPublic)
                            return (int)IconEnum.PublicMethod;
                        else if (method.IsAssembly)
                            return (int)IconEnum.InternalMethod;
                        else if (method.IsFamilyAndAssembly)
                            return (int)IconEnum.ProtectedInternalMethod;
                    }
                    else
                    {
                        if (method.IsPrivate)
                            return (int)IconEnum.StaticPrivateMethod;
                        else if (method.IsFamily)
                            return (int)IconEnum.StaticProtectedMethod;
                        else if (method.IsPublic)
                            return (int)IconEnum.StaticPublicMethod;
                        else if (method.IsAssembly)
                            return (int)IconEnum.StaticInternalMethod;
                        else if (method.IsFamilyAndAssembly)
                            return (int)IconEnum.StaticProtectedInternalMethod;
                    }
                }
            }
            else if (member is ConstructorInfo)
            {
                ConstructorInfo ctor = (ConstructorInfo)member;

                if (!ctor.IsStatic)
                {
                    if (ctor.IsPrivate)
                        return (int)IconEnum.PrivateConstructor;
                    else if (ctor.IsFamily)
                        return (int)IconEnum.ProtectedConstructor;
                    else if (ctor.IsPublic)
                        return (int)IconEnum.PublicConstructor;
                    else if (ctor.IsAssembly)
                        return (int)IconEnum.InternalConstructor;
                    else if (ctor.IsFamilyAndAssembly)
                        return (int)IconEnum.ProtectedInternalConstructor;
                }
                else
                {
                    if (ctor.IsPrivate)
                        return (int)IconEnum.StaticPrivateConstructor;
                    else if (ctor.IsFamily)
                        return (int)IconEnum.StaticProtectedConstructor;
                    else if (ctor.IsPublic)
                        return (int)IconEnum.StaticPublicConstructor;
                    else if (ctor.IsAssembly)
                        return (int)IconEnum.StaticInternalConstructor;
                    else if (ctor.IsFamilyAndAssembly)
                        return (int)IconEnum.StaticProtectedInternalConstructor;
                }
            }

            return -1;
        }

    }
}
