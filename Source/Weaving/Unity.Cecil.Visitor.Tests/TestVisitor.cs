using System.Linq;
using Mono.Cecil;
using NUnit.Framework;

namespace Unity.Cecil.Visitor.Tests
{
	[TestFixture]
	public class TestVisitor
	{
		[Test]
		public void DontCrashWithNullVisitor()
		{
			ThisAssembly.Accept(null);
		}

		[Test]
		public void CanVisitAssemblyDefinition()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouched(ThisAssembly));
		}

		[Test]
		public void CanVisitModuleDefinition()
		{
			var visitor = new TracingVisitor();

			ThisModule.Accept(visitor);

			Assert.IsTrue(visitor.HasTouched(ThisModule));
		}

		[Test]
		public void CanVisitTypeDefinition()
		{
			var visitor = new TracingVisitor();
			var fixtureType = ThisFixtureType;

			fixtureType.Accept(visitor);

			Assert.IsTrue(visitor.HasTouched(fixtureType));
		}

		[Test]
		public void CanVisitTypeDefinitionsInTree()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouched(ThisAssembly));
			Assert.IsTrue(visitor.HasTouched(ThisModule));
			Assert.IsTrue(visitor.HasTouched(ThisFixtureType));
		}

		[Test]
		public void CanVisitNestedTypesInTree()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouched(ThisAssembly));
			Assert.IsTrue(visitor.HasTouched(ThisModule));
			Assert.IsTrue(visitor.HasTouched(ThisFixtureType));
			Assert.IsTrue(visitor.HasTouched(ThisNestedType));
		}

		[Test]
		public void CanVisitBaseTypes()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouched(ThisFixtureType.BaseType));
			Assert.IsTrue(visitor.HasTouched(ThisNestedType.BaseType));
		}

		[Test]
		public void CanVisitInterfaces()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisFixtureType.Interfaces));
			Assert.IsTrue(visitor.HasTouchedAll(ThisNestedType.Interfaces));
		}

		[Test]
		public void CanVisitFieldDefinitions()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisFixtureType.Fields));
			Assert.IsTrue(visitor.HasTouchedAll(ThisNestedType.Fields));
		}

		[Test]
		public void CanVisitEventDefinitions()
		{
			var visitor = new TracingVisitor();

			ThisFixtureType.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisFixtureType.Events));
		}

		[Test]
		public void CanVisitMethodDefinitions()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisFixtureType.Methods));
			Assert.IsTrue(visitor.HasTouchedAll(ThisNestedType.Methods));
			Assert.IsTrue(visitor.HasTouchedAll(ThisEmptyInterface.Methods));
		}

		[Test]
		public void CanVisitGenericParameters()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisGeneric.GenericParameters));
		}

		[Test]
		public void CanVisitComposedTypes()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisComposedTypes.Fields.Select(f => f.FieldType)));
			Assert.IsTrue(visitor.HasTouchedAll(ThisComposedTypes.Fields.Select(f => f.FieldType.GetElementType())));
		}

		[Test]
		public void CanVisitMethodParameters()
		{
			var visitor = new TracingVisitor();

			ThisAssembly.Accept(visitor);

			Assert.IsTrue(visitor.HasTouchedAll(ThisModule.Types.SelectMany(t => t.Methods.SelectMany(m => m.Parameters))));
		}

		[Test]
		[Ignore("WIP")]
		public void CanVisitMscorlib()
		{
			var visitor = new TracingVisitor();

			MSCorlibAssembly.Accept(visitor);

			// TODO shitty test, but better than nothing - I have to catch the flight now

			Assert.AreEqual(2783, visitor.Touched.OfType<TypeDefinition>().Count());
			Assert.AreEqual(25362, visitor.Touched.OfType<TypeReference>().Count());
			Assert.AreEqual(12632, visitor.Touched.OfType<FieldDefinition>().Count());
			Assert.AreEqual(12454, visitor.Touched.OfType<CustomAttribute>().Count());
		}

		private static AssemblyDefinition _thisAssembly;
		private static AssemblyDefinition _mscorlibAssembly;

		private static AssemblyDefinition MSCorlibAssembly
		{
			get { return _mscorlibAssembly ?? (_mscorlibAssembly = AssemblyDefinition.ReadAssembly(typeof(object).Assembly.Location)); }
		}

		private static AssemblyDefinition ThisAssembly
		{
			get { return _thisAssembly ?? (_thisAssembly = AssemblyDefinition.ReadAssembly(typeof(TestVisitor).Assembly.Location)); }
		}

		private static ModuleDefinition ThisModule
		{
			get { return ThisAssembly.MainModule; }
		}

		private static TypeDefinition ThisFixtureType
		{
			get { return ThisModule.Types.Single(t => t.FullName == "Unity.Cecil.Visitor.Tests.Fixture"); }
		}

		private static TypeDefinition ThisEmptyInterface
		{
			get { return ThisModule.Types.Single(t => t.FullName == "Unity.Cecil.Visitor.Tests.IEmptyInterface"); }
		}

		private static TypeDefinition ThisNestedType
		{
			get { return ThisFixtureType.NestedTypes.Single(t => t.FullName == "Unity.Cecil.Visitor.Tests.Fixture/Nested"); }
		}

		private static TypeDefinition ThisGeneric
		{
			get { return ThisModule.Types.Single(t => t.FullName == "Unity.Cecil.Visitor.Tests.Generic`1"); }
		}

		private static TypeDefinition ThisComposedTypes
		{
			get { return ThisModule.Types.Single(t => t.FullName == "Unity.Cecil.Visitor.Tests.ComposedTypes"); }
		}
	}
}
