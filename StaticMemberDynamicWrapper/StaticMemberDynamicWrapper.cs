using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace StaticMemberDynamicWrapper
{
    internal sealed class StaticMemberDynamicWrapper : DynamicObject
    {
        private readonly TypeInfo m_type;

        public StaticMemberDynamicWrapper(Type type)
        {
            m_type = type.GetTypeInfo();
        }

        public override IEnumerable<string> GetDynamicMemberNames() => m_type.DeclaredMembers.Select(mi => mi.Name);

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var field = FindField(binder.Name);
            if (field != null)
            {
                result = field.GetValue(null);
                return true;
            }

            var prop = FindProperty(binder.Name, true);
            if (prop != null)
            {
                result = prop.GetValue(null, null);
                return true;
            }

            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var field = FindField(binder.Name);

            if (field != null)
            {
                field.SetValue(null, value);
                return true;
            }

            var prop = FindProperty(binder.Name, false);
            if (prop != null)
            {
                prop.SetValue(null, value, null);
                return true;
            }

            return false;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var paramTypes = args.Select(arg => arg.GetType()).ToArray();
            MethodInfo method = FindMethod(binder.Name, paramTypes);
            if (method == null)
            {
                result = null;
                return false;
            }

            result = method.Invoke(null, args);
            return true;
        }

        private MethodInfo FindMethod(string name, IReadOnlyList<Type> paramTypes)
        {
            return m_type.DeclaredMethods.FirstOrDefault(mi => mi.IsPublic && mi.IsStatic
                                                                           && mi.Name == name
                                                                           && ParametersMatch(mi.GetParameters(),
                                                                               paramTypes));
        }

        private static bool ParametersMatch(IReadOnlyList<ParameterInfo> parameters, IReadOnlyList<Type> paramTypes)
        {
            if (parameters.Count != paramTypes.Count)
                return false;

            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].ParameterType != paramTypes[i])
                    return false;
            }

            return true;
        }

        private FieldInfo FindField(string name)
        {
            return m_type.DeclaredFields.FirstOrDefault(fi => fi.IsPublic && fi.IsStatic && fi.Name == name);
        }

        private PropertyInfo FindProperty(string name, bool get)
        {
            if (get)
            {
                return m_type.DeclaredProperties.FirstOrDefault(pi => pi.Name == name && pi.GetMethod != null &&
                                                                      pi.GetMethod.IsPublic && pi.GetMethod.IsStatic);
            }

            return m_type.DeclaredProperties.FirstOrDefault(pi => pi.Name == name && pi.SetMethod != null &&
                                                                  pi.SetMethod.IsPublic && pi.SetMethod.IsStatic);
        }
    }
}
