using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Unity.Cecil.Visitor.Tests
{
	class TracingVisitor: Visitor
	{
		private readonly HashSet<object> _touched = new HashSet<object>();

		public IEnumerable<object> Touched
		{
			get { return _touched; }
		}

		public bool HasTouched<T>(T definition)
		{
			return _touched.Contains(definition);
		}

		public bool HasTouchedAll<T>(IEnumerable<T> values)
		{
			return values.All(HasTouched);
		}

		protected override void Visit(AssemblyDefinition assemblyDefinition, Context context)
		{
			Touch(assemblyDefinition);

			base.Visit(assemblyDefinition, context);
		}

		protected override void Visit(ModuleDefinition moduleDefinition, Context context)
		{
			Touch(moduleDefinition);

			base.Visit(moduleDefinition, context);
		}

		protected override void Visit(TypeDefinition typeDefinition, Context context)
		{
			Touch(typeDefinition);

			base.Visit(typeDefinition, context);
		}

		protected override void Visit(TypeReference typeReference, Context context)
		{
			Touch(typeReference);

			base.Visit(typeReference, context);
		}

		protected override void Visit(FieldDefinition fieldDefinition, Context context)
		{
			Touch(fieldDefinition);

			base.Visit(fieldDefinition, context);
		}

		protected override void Visit(EventDefinition eventDefinition, Context context)
		{
			Touch(eventDefinition);

			base.Visit(eventDefinition, context);
		}

		protected override void Visit(MethodDefinition methodDefinition, Context context)
		{
			Touch(methodDefinition);

			base.Visit(methodDefinition, context);
		}

		protected override void Visit(GenericParameter genericParameter, Context context)
		{
			Touch(genericParameter);

			base.Visit(genericParameter, context);
		}

		protected override void Visit(ArrayType arrayType, Context context)
		{
			Touch(arrayType);

			base.Visit(arrayType, context);
		}

		protected override void Visit(ByReferenceType byReferenceType, Context context)
		{
			Touch(byReferenceType);

			base.Visit(byReferenceType, context);
		}

		protected override void Visit(FunctionPointerType functionPointerType, Context context)
		{
			Touch(functionPointerType);

			base.Visit(functionPointerType, context);
		}

		protected override void Visit(GenericInstanceType genericInstanceType, Context context)
		{
			Touch(genericInstanceType);

			base.Visit(genericInstanceType, context);
		}

		protected override void Visit(PinnedType pinnedType, Context context)
		{
			Touch(pinnedType);

			base.Visit(pinnedType, context);
		}

		protected override void Visit(PointerType pointerType, Context context)
		{
			Touch(pointerType);

			base.Visit(pointerType, context);
		}

		protected override void Visit(PropertyDefinition propertyDefinition, Context context)
		{
			Touch(propertyDefinition);

			base.Visit(propertyDefinition, context);
		}

		protected override void Visit(SentinelType sentinelType, Context context)
		{
			Touch(sentinelType);

			base.Visit(sentinelType, context);
		}

		protected override void Visit(MethodReference methodReference, Context context)
		{
			Touch(methodReference);

			base.Visit(methodReference, context);
		}

		protected override void Visit(ParameterDefinition parameterDefinition, Context context)
		{
			Touch(parameterDefinition);

			base.Visit(parameterDefinition, context);
		}

		protected override void Visit(CustomAttribute customAttribute, Context context)
		{
			Touch(customAttribute);

			base.Visit(customAttribute, context);
		}

		protected override void Visit(CustomAttributeArgument customAttributeArgument, Context context)
		{
			Touch(customAttributeArgument);

			base.Visit(customAttributeArgument, context);
		}

		protected override void Visit(CustomAttributeNamedArgument customAttributeNamedArgument, Context context)
		{
			Touch(customAttributeNamedArgument);

			base.Visit(customAttributeNamedArgument, context);
		}

		protected override void Visit(FieldReference fieldReference, Context context)
		{
			Touch(fieldReference);

			base.Visit(fieldReference, context);
		}

		protected override void Visit(Mono.Cecil.Cil.ExceptionHandler exceptionHandler, Context context)
		{
			Touch(exceptionHandler);

			base.Visit(exceptionHandler, context);
		}

		protected override void Visit(Mono.Cecil.Cil.Instruction instruction, Context context)
		{
			Touch(instruction);

			base.Visit(instruction, context);
		}

		protected override void Visit(Mono.Cecil.Cil.MethodBody methodBody, Context context)
		{
			Touch(methodBody);

			base.Visit(methodBody, context);
		}

		protected override void Visit(Mono.Cecil.Cil.VariableDefinition variableDefinition, Context context)
		{
			Touch(variableDefinition);

			base.Visit(variableDefinition, context);
		}

		private void Touch<T>(T value)
		{
			_touched.Add(value);
		}
	}
}