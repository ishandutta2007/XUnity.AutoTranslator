﻿using Il2CppSystem.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;
using UnityEngine;

namespace XUnity.Common.Utilities
{
   public static class Il2CppUtilities
   {
      private static Dictionary<string, IntPtr> ourImagesMap;

      public static Func<object, uint> GetGarbageCollectionHandle =
         CustomFastReflectionHelper.CreateFastFieldGetter<object, uint>(
            typeof( Il2CppObjectBase ).GetField(
               "myGcHandle",
               BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) );

      public static readonly Func<IntPtr, Component> CreateProxyComponent =
         (Func<IntPtr, Component>)ExpressionHelper.CreateTypedFastInvoke(
            typeof( Component ).GetConstructor(
               BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
               null,
               new Type[] { typeof( IntPtr ) },
               null
            ) );

      public static IntPtr GetIl2CppInstancePointer( object obj )
      {
         var gcHandle = GetGarbageCollectionHandle( obj );
         var intPtr = UnhollowerBaseLib.IL2CPP.il2cpp_gchandle_get_target( gcHandle );
         if( intPtr == IntPtr.Zero )
         {
            throw new ObjectCollectedException( "Object was garbage collected in IL2CPP domain" );
         }
         return intPtr;
      }

      public static IntPtr GetIl2CppInstancePointer( uint gcHandle )
      {
         var intPtr = UnhollowerBaseLib.IL2CPP.il2cpp_gchandle_get_target( gcHandle );
         if( intPtr == IntPtr.Zero )
         {
            throw new ObjectCollectedException( "Object was garbage collected in IL2CPP domain" );
         }
         return intPtr;
      }

      public static IntPtr GetIl2CppClass( string namespaze, string className )
      {
         if( ourImagesMap == null )
         {
            ourImagesMap = (Dictionary<string, IntPtr>)typeof( UnhollowerBaseLib.IL2CPP ).GetField( "ourImagesMap", BindingFlags.NonPublic | BindingFlags.Static ).GetValue( null );
         }

         foreach( var image in ourImagesMap.Values )
         {
            var clazz = UnhollowerBaseLib.IL2CPP.il2cpp_class_from_name( image, namespaze, className );

            if( clazz != IntPtr.Zero )
               return clazz;
         }

         return IntPtr.Zero;
      }

      public static IntPtr GetIl2CppMethod( IntPtr? clazz, string methodName, Type returnType, params Type[] types )
      {
         try
         {
            if( !clazz.HasValue || clazz == IntPtr.Zero ) return IntPtr.Zero;

            return UnhollowerBaseLib.IL2CPP.GetIl2CppMethod( clazz.Value, false, methodName, returnType.FullName, types.Select( x => x.FullName ).ToArray() );
         }
         catch
         {
            return IntPtr.Zero;
         }
      }

      public static IntPtr GetIl2CppMethod( IntPtr? clazz, string methodName, string returnType, params string[] types )
      {
         if( !clazz.HasValue || clazz == IntPtr.Zero ) return IntPtr.Zero;

         return UnhollowerBaseLib.IL2CPP.GetIl2CppMethod( clazz.Value, false, methodName, returnType, types );
      }

      unsafe public static IntPtr InvokeMethod( IntPtr method, IntPtr obj, params IntPtr[] paramtbl )
      {
         if( method == IntPtr.Zero )
            return IntPtr.Zero;
         IntPtr[] intPtrArray;
         IntPtr returnval = IntPtr.Zero;
         intPtrArray = ( ( paramtbl != null ) ? paramtbl : new IntPtr[ 0 ] );
         IntPtr intPtr = Marshal.AllocHGlobal( intPtrArray.Length * sizeof( void* ) );
         try
         {
            void** pointerArray = (void**)intPtr.ToPointer();
            for( int i = 0; i < intPtrArray.Length; i++ )
               pointerArray[ i ] = intPtrArray[ i ].ToPointer();
            IntPtr exp = IntPtr.Zero;
            returnval = UnhollowerBaseLib.IL2CPP.il2cpp_runtime_invoke( method, obj, pointerArray, ref exp );
            Il2CppException.RaiseExceptionIfNecessary( exp );
         }
         finally
         {
            Marshal.FreeHGlobal( intPtr );
         }
         return returnval;
      }

      unsafe public static bool PointerToManagedBool( IntPtr ptr )
      {
         return *(bool*)(long)UnhollowerBaseLib.IL2CPP.il2cpp_object_unbox( ptr );
      }
   }
}
