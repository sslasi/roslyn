﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Semantics
{
    internal sealed partial class CSharpOperationFactory
    {
        private readonly ConcurrentDictionary<BoundNode, IOperation> _cache =
            new ConcurrentDictionary<BoundNode, IOperation>(concurrencyLevel: 2, capacity: 10);
        private readonly SemanticModel _semanticModel;

        public CSharpOperationFactory(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public IOperation Create(BoundNode boundNode)
        {
            if (boundNode == null)
            {
                return null;
            }

            return _cache.GetOrAdd(boundNode, n => CreateInternal(n));
        }

        private IOperation CreateInternal(BoundNode boundNode)
        {
            switch (boundNode.Kind)
            {
                case BoundKind.DeconstructValuePlaceholder:
                    return CreateBoundDeconstructValuePlaceholderOperation((BoundDeconstructValuePlaceholder)boundNode);
                case BoundKind.Call:
                    return CreateBoundCallOperation((BoundCall)boundNode);
                case BoundKind.Local:
                    return CreateBoundLocalOperation((BoundLocal)boundNode);
                case BoundKind.FieldAccess:
                    return CreateBoundFieldAccessOperation((BoundFieldAccess)boundNode);
                case BoundKind.PropertyAccess:
                    return CreateBoundPropertyAccessOperation((BoundPropertyAccess)boundNode);
                case BoundKind.IndexerAccess:
                    return CreateBoundIndexerAccessOperation((BoundIndexerAccess)boundNode);
                case BoundKind.EventAccess:
                    return CreateBoundEventAccessOperation((BoundEventAccess)boundNode);
                case BoundKind.EventAssignmentOperator:
                    return CreateBoundEventAssignmentOperatorOperation((BoundEventAssignmentOperator)boundNode);
                case BoundKind.Parameter:
                    return CreateBoundParameterOperation((BoundParameter)boundNode);
                case BoundKind.Literal:
                    return CreateBoundLiteralOperation((BoundLiteral)boundNode);
                case BoundKind.ObjectCreationExpression:
                    return CreateBoundObjectCreationExpressionOperation((BoundObjectCreationExpression)boundNode);
                case BoundKind.UnboundLambda:
                    return CreateUnboundLambdaOperation((UnboundLambda)boundNode);
                case BoundKind.Lambda:
                    return CreateBoundLambdaOperation((BoundLambda)boundNode);
                case BoundKind.Conversion:
                    return CreateBoundConversionOperation((BoundConversion)boundNode);
                case BoundKind.AsOperator:
                    return CreateBoundAsOperatorOperation((BoundAsOperator)boundNode);
                case BoundKind.IsOperator:
                    return CreateBoundIsOperatorOperation((BoundIsOperator)boundNode);
                case BoundKind.SizeOfOperator:
                    return CreateBoundSizeOfOperatorOperation((BoundSizeOfOperator)boundNode);
                case BoundKind.TypeOfOperator:
                    return CreateBoundTypeOfOperatorOperation((BoundTypeOfOperator)boundNode);
                case BoundKind.ArrayCreation:
                    return CreateBoundArrayCreationOperation((BoundArrayCreation)boundNode);
                case BoundKind.ArrayInitialization:
                    return CreateBoundArrayInitializationOperation((BoundArrayInitialization)boundNode);
                case BoundKind.DefaultExpression:
                    return CreateBoundDefaultExpressionOperation((BoundDefaultExpression)boundNode);
                case BoundKind.BaseReference:
                    return CreateBoundBaseReferenceOperation((BoundBaseReference)boundNode);
                case BoundKind.ThisReference:
                    return CreateBoundThisReferenceOperation((BoundThisReference)boundNode);
                case BoundKind.AssignmentOperator:
                    return CreateBoundAssignmentOperatorOperation((BoundAssignmentOperator)boundNode);
                case BoundKind.CompoundAssignmentOperator:
                    return CreateBoundCompoundAssignmentOperatorOperation((BoundCompoundAssignmentOperator)boundNode);
                case BoundKind.IncrementOperator:
                    return CreateBoundIncrementOperatorOperation((BoundIncrementOperator)boundNode);
                case BoundKind.BadExpression:
                    return CreateBoundBadExpressionOperation((BoundBadExpression)boundNode);
                case BoundKind.NewT:
                    return CreateBoundNewTOperation((BoundNewT)boundNode);
                case BoundKind.UnaryOperator:
                    return CreateBoundUnaryOperatorOperation((BoundUnaryOperator)boundNode);
                case BoundKind.BinaryOperator:
                    return CreateBoundBinaryOperatorOperation((BoundBinaryOperator)boundNode);
                case BoundKind.ConditionalOperator:
                    return CreateBoundConditionalOperatorOperation((BoundConditionalOperator)boundNode);
                case BoundKind.NullCoalescingOperator:
                    return CreateBoundNullCoalescingOperatorOperation((BoundNullCoalescingOperator)boundNode);
                case BoundKind.AwaitExpression:
                    return CreateBoundAwaitExpressionOperation((BoundAwaitExpression)boundNode);
                case BoundKind.ArrayAccess:
                    return CreateBoundArrayAccessOperation((BoundArrayAccess)boundNode);
                case BoundKind.PointerIndirectionOperator:
                    return CreateBoundPointerIndirectionOperatorOperation((BoundPointerIndirectionOperator)boundNode);
                case BoundKind.NameOfOperator:
                    return CreateBoundNameOfOperatorOperation((BoundNameOfOperator)boundNode);
                case BoundKind.ThrowExpression:
                    return CreateBoundThrowExpressionOperation((BoundThrowExpression)boundNode);
                case BoundKind.AddressOfOperator:
                    return CreateBoundAddressOfOperatorOperation((BoundAddressOfOperator)boundNode);
                case BoundKind.ImplicitReceiver:
                    return CreateBoundImplicitReceiverOperation((BoundImplicitReceiver)boundNode);
                case BoundKind.ConditionalAccess:
                    return CreateBoundConditionalAccessOperation((BoundConditionalAccess)boundNode);
                case BoundKind.ConditionalReceiver:
                    return CreateBoundConditionalReceiverOperation((BoundConditionalReceiver)boundNode);
                case BoundKind.FieldEqualsValue:
                    return CreateBoundFieldEqualsValueOperation((BoundFieldEqualsValue)boundNode);
                case BoundKind.PropertyEqualsValue:
                    return CreateBoundPropertyEqualsValueOperation((BoundPropertyEqualsValue)boundNode);
                case BoundKind.ParameterEqualsValue:
                    return CreateBoundParameterEqualsValueOperation((BoundParameterEqualsValue)boundNode);
                case BoundKind.Block:
                    return CreateBoundBlockOperation((BoundBlock)boundNode);
                case BoundKind.ContinueStatement:
                    return CreateBoundContinueStatementOperation((BoundContinueStatement)boundNode);
                case BoundKind.BreakStatement:
                    return CreateBoundBreakStatementOperation((BoundBreakStatement)boundNode);
                case BoundKind.YieldBreakStatement:
                    return CreateBoundYieldBreakStatementOperation((BoundYieldBreakStatement)boundNode);
                case BoundKind.GotoStatement:
                    return CreateBoundGotoStatementOperation((BoundGotoStatement)boundNode);
                case BoundKind.NoOpStatement:
                    return CreateBoundNoOpStatementOperation((BoundNoOpStatement)boundNode);
                case BoundKind.IfStatement:
                    return CreateBoundIfStatementOperation((BoundIfStatement)boundNode);
                case BoundKind.WhileStatement:
                    return CreateBoundWhileStatementOperation((BoundWhileStatement)boundNode);
                case BoundKind.DoStatement:
                    return CreateBoundDoStatementOperation((BoundDoStatement)boundNode);
                case BoundKind.ForStatement:
                    return CreateBoundForStatementOperation((BoundForStatement)boundNode);
                case BoundKind.ForEachStatement:
                    return CreateBoundForEachStatementOperation((BoundForEachStatement)boundNode);
                case BoundKind.SwitchStatement:
                    return CreateBoundSwitchStatementOperation((BoundSwitchStatement)boundNode);
                case BoundKind.SwitchLabel:
                    return CreateBoundSwitchLabelOperation((BoundSwitchLabel)boundNode);
                case BoundKind.TryStatement:
                    return CreateBoundTryStatementOperation((BoundTryStatement)boundNode);
                case BoundKind.CatchBlock:
                    return CreateBoundCatchBlockOperation((BoundCatchBlock)boundNode);
                case BoundKind.FixedStatement:
                    return CreateBoundFixedStatementOperation((BoundFixedStatement)boundNode);
                case BoundKind.UsingStatement:
                    return CreateBoundUsingStatementOperation((BoundUsingStatement)boundNode);
                case BoundKind.ThrowStatement:
                    return CreateBoundThrowStatementOperation((BoundThrowStatement)boundNode);
                case BoundKind.ReturnStatement:
                    return CreateBoundReturnStatementOperation((BoundReturnStatement)boundNode);
                case BoundKind.YieldReturnStatement:
                    return CreateBoundYieldReturnStatementOperation((BoundYieldReturnStatement)boundNode);
                case BoundKind.LockStatement:
                    return CreateBoundLockStatementOperation((BoundLockStatement)boundNode);
                case BoundKind.BadStatement:
                    return CreateBoundBadStatementOperation((BoundBadStatement)boundNode);
                case BoundKind.LocalDeclaration:
                    return CreateBoundLocalDeclarationOperation((BoundLocalDeclaration)boundNode);
                case BoundKind.MultipleLocalDeclarations:
                    return CreateBoundMultipleLocalDeclarationsOperation((BoundMultipleLocalDeclarations)boundNode);
                case BoundKind.LabelStatement:
                    return CreateBoundLabelStatementOperation((BoundLabelStatement)boundNode);
                case BoundKind.LabeledStatement:
                    return CreateBoundLabeledStatementOperation((BoundLabeledStatement)boundNode);
                case BoundKind.ExpressionStatement:
                    return CreateBoundExpressionStatementOperation((BoundExpressionStatement)boundNode);
                case BoundKind.InterpolatedString:
                    return CreateBoundInterpolatedStringExpressionOperation((BoundInterpolatedString)boundNode);
                case BoundKind.StringInsert:
                    return CreateBoundInterpolationOperation((BoundStringInsert)boundNode);
                case BoundKind.LocalFunctionStatement:
                    return CreateBoundLocalFunctionStatementOperation((BoundLocalFunctionStatement)boundNode);
                case BoundKind.AnonymousObjectCreationExpression:
                    return CreateBoundAnonymousObjectCreationExpressionOperation((BoundAnonymousObjectCreationExpression)boundNode);
                case BoundKind.AnonymousPropertyDeclaration:
                    return CreateBoundAnonymousPropertyDeclarationOperation((BoundAnonymousPropertyDeclaration)boundNode);
                case BoundKind.ConstantPattern:
                    return CreateBoundConstantPatternOperation((BoundConstantPattern)boundNode);
                case BoundKind.DeclarationPattern:
                    return CreateBoundDeclarationPatternOperation((BoundDeclarationPattern)boundNode);
                case BoundKind.WildcardPattern:
                    throw ExceptionUtilities.Unreachable;
                case BoundKind.PatternSwitchStatement:
                    return CreateBoundPatternSwitchStatementOperation((BoundPatternSwitchStatement)boundNode);
                case BoundKind.PatternSwitchLabel:
                    return CreateBoundPatternSwitchLabelOperation((BoundPatternSwitchLabel)boundNode);
                case BoundKind.IsPatternExpression:
                    return CreateBoundIsPatternExpressionOperation((BoundIsPatternExpression)boundNode);
                default:
                    var constantValue = ConvertToOptional((boundNode as BoundExpression)?.ConstantValue);
                    return Operation.CreateOperationNone(boundNode.Syntax, constantValue, getChildren: () => GetIOperationChildren(boundNode));
            }
        }

        private ImmutableArray<IOperation> GetIOperationChildren(BoundNode boundNode)
        {
            var boundNodeWithChildren = (IBoundNodeWithIOperationChildren)boundNode;
            if (boundNodeWithChildren.Children.IsDefaultOrEmpty)
            {
                return ImmutableArray<IOperation>.Empty;
            }

            var builder = ArrayBuilder<IOperation>.GetInstance(boundNodeWithChildren.Children.Length);
            foreach (var childNode in boundNodeWithChildren.Children)
            {
                var operation = Create(childNode);
                builder.Add(operation);
            }

            return builder.ToImmutableAndFree();
        }

        private IPlaceholderExpression CreateBoundDeconstructValuePlaceholderOperation(BoundDeconstructValuePlaceholder boundDeconstructValuePlaceholder)
        {
            SyntaxNode syntax = boundDeconstructValuePlaceholder.Syntax;
            ITypeSymbol type = boundDeconstructValuePlaceholder.Type;
            Optional<object> constantValue = ConvertToOptional(boundDeconstructValuePlaceholder.ConstantValue);
            return new PlaceholderExpression(syntax, type, constantValue);
        }

        private IInvocationExpression CreateBoundCallOperation(BoundCall boundCall)
        {
            IMethodSymbol targetMethod = boundCall.Method;
            Lazy<IOperation> instance = new Lazy<IOperation>(() => Create(((object)boundCall.Method == null || boundCall.Method.IsStatic) ? null : boundCall.ReceiverOpt));
            bool isVirtual = (object)boundCall.Method != null &&
                        boundCall.ReceiverOpt != null &&
                        (boundCall.Method.IsVirtual || boundCall.Method.IsAbstract || boundCall.Method.IsOverride) &&
                        !boundCall.ReceiverOpt.SuppressVirtualCalls;
            Lazy<ImmutableArray<IArgument>> argumentsInEvaluationOrder = new Lazy<ImmutableArray<IArgument>>(() => DeriveArguments(boundCall, boundCall.BinderOpt, boundCall.Method, boundCall.Method, boundCall.Arguments, boundCall.ArgumentNamesOpt, boundCall.ArgsToParamsOpt, boundCall.ArgumentRefKindsOpt, boundCall.Method.Parameters, boundCall.Expanded, boundCall.Syntax, boundCall.InvokedAsExtensionMethod));
            SyntaxNode syntax = boundCall.Syntax;
            ITypeSymbol type = boundCall.Type;
            Optional<object> constantValue = ConvertToOptional(boundCall.ConstantValue);
            return new LazyInvocationExpression(targetMethod, instance, isVirtual, argumentsInEvaluationOrder, syntax, type, constantValue);
        }

        private ILocalReferenceExpression CreateBoundLocalOperation(BoundLocal boundLocal)
        {
            ILocalSymbol local = boundLocal.LocalSymbol;
            SyntaxNode syntax = boundLocal.Syntax;
            ITypeSymbol type = boundLocal.Type;
            Optional<object> constantValue = ConvertToOptional(boundLocal.ConstantValue);
            return new LocalReferenceExpression(local, syntax, type, constantValue);
        }

        private IFieldReferenceExpression CreateBoundFieldAccessOperation(BoundFieldAccess boundFieldAccess)
        {
            IFieldSymbol field = boundFieldAccess.FieldSymbol;
            Lazy<IOperation> instance = new Lazy<IOperation>(() => Create(boundFieldAccess.FieldSymbol.IsStatic ? null : boundFieldAccess.ReceiverOpt));
            ISymbol member = boundFieldAccess.FieldSymbol;
            SyntaxNode syntax = boundFieldAccess.Syntax;
            ITypeSymbol type = boundFieldAccess.Type;
            Optional<object> constantValue = ConvertToOptional(boundFieldAccess.ConstantValue);
            return new LazyFieldReferenceExpression(field, instance, member, syntax, type, constantValue);
        }

        private IPropertyReferenceExpression CreateBoundPropertyAccessOperation(BoundPropertyAccess boundPropertyAccess)
        {
            IPropertySymbol property = boundPropertyAccess.PropertySymbol;
            Lazy<IOperation> instance = new Lazy<IOperation>(() => Create(boundPropertyAccess.PropertySymbol.IsStatic ? null : boundPropertyAccess.ReceiverOpt));
            Lazy<ImmutableArray<IArgument>> argumentsInEvaluationOrder = new Lazy<ImmutableArray<IArgument>>(() => ImmutableArray<IArgument>.Empty);
            ISymbol member = boundPropertyAccess.PropertySymbol;
            SyntaxNode syntax = boundPropertyAccess.Syntax;
            ITypeSymbol type = boundPropertyAccess.Type;
            Optional<object> constantValue = ConvertToOptional(boundPropertyAccess.ConstantValue);
            return new LazyPropertyReferenceExpression(property, instance, member, argumentsInEvaluationOrder, syntax, type, constantValue);
        }

        private IPropertyReferenceExpression CreateBoundIndexerAccessOperation(BoundIndexerAccess boundIndexerAccess)
        {
            IPropertySymbol property = boundIndexerAccess.Indexer;
            Lazy<IOperation> instance = new Lazy<IOperation>(() => Create(boundIndexerAccess.Indexer.IsStatic ? null : boundIndexerAccess.ReceiverOpt));
            ISymbol member = boundIndexerAccess.Indexer;
            Lazy<ImmutableArray<IArgument>> argumentsInEvaluationOrder = new Lazy<ImmutableArray<IArgument>>(() =>
            {
                MethodSymbol accessor = boundIndexerAccess.UseSetterForDefaultArgumentGeneration
                    ? boundIndexerAccess.Indexer.GetOwnOrInheritedSetMethod()
                    : boundIndexerAccess.Indexer.GetOwnOrInheritedGetMethod();

                return DeriveArguments(boundIndexerAccess,
                    boundIndexerAccess.BinderOpt,
                    boundIndexerAccess.Indexer,
                    accessor,
                    boundIndexerAccess.Arguments,
                    boundIndexerAccess.ArgumentNamesOpt,
                    boundIndexerAccess.ArgsToParamsOpt,
                    boundIndexerAccess.ArgumentRefKindsOpt,
                    boundIndexerAccess.Indexer.Parameters,
                    boundIndexerAccess.Expanded,
                    boundIndexerAccess.Syntax);
            });
            SyntaxNode syntax = boundIndexerAccess.Syntax;
            ITypeSymbol type = boundIndexerAccess.Type;
            Optional<object> constantValue = ConvertToOptional(boundIndexerAccess.ConstantValue);
            return new LazyPropertyReferenceExpression(property, instance, member, argumentsInEvaluationOrder, syntax, type, constantValue);
        }

        private IEventReferenceExpression CreateBoundEventAccessOperation(BoundEventAccess boundEventAccess)
        {
            IEventSymbol @event = boundEventAccess.EventSymbol;
            Lazy<IOperation> instance = new Lazy<IOperation>(() => Create(boundEventAccess.EventSymbol.IsStatic ? null : boundEventAccess.ReceiverOpt));
            ISymbol member = boundEventAccess.EventSymbol;
            SyntaxNode syntax = boundEventAccess.Syntax;
            ITypeSymbol type = boundEventAccess.Type;
            Optional<object> constantValue = ConvertToOptional(boundEventAccess.ConstantValue);
            return new LazyEventReferenceExpression(@event, instance, member, syntax, type, constantValue);
        }

        private IEventAssignmentExpression CreateBoundEventAssignmentOperatorOperation(BoundEventAssignmentOperator boundEventAssignmentOperator)
        {
            IEventSymbol @event = boundEventAssignmentOperator.Event;
            Lazy<IOperation> eventInstance = new Lazy<IOperation>(() => Create(boundEventAssignmentOperator.Event.IsStatic ? null : boundEventAssignmentOperator.ReceiverOpt));
            Lazy<IOperation> handlerValue = new Lazy<IOperation>(() => Create(boundEventAssignmentOperator.Argument));
            bool adds = boundEventAssignmentOperator.IsAddition;
            SyntaxNode syntax = boundEventAssignmentOperator.Syntax;
            ITypeSymbol type = boundEventAssignmentOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundEventAssignmentOperator.ConstantValue);
            return new LazyEventAssignmentExpression(@event, eventInstance, handlerValue, adds, syntax, type, constantValue);
        }

        private IParameterReferenceExpression CreateBoundParameterOperation(BoundParameter boundParameter)
        {
            IParameterSymbol parameter = boundParameter.ParameterSymbol;
            SyntaxNode syntax = boundParameter.Syntax;
            ITypeSymbol type = boundParameter.Type;
            Optional<object> constantValue = ConvertToOptional(boundParameter.ConstantValue);
            return new ParameterReferenceExpression(parameter, syntax, type, constantValue);
        }

        private ILiteralExpression CreateBoundLiteralOperation(BoundLiteral boundLiteral)
        {
            string text = boundLiteral.Syntax.ToString();
            SyntaxNode syntax = boundLiteral.Syntax;
            ITypeSymbol type = boundLiteral.Type;
            Optional<object> constantValue = ConvertToOptional(boundLiteral.ConstantValue);
            return new LiteralExpression(text, syntax, type, constantValue);
        }

        private IAnonymousObjectCreationExpression CreateBoundAnonymousObjectCreationExpressionOperation(BoundAnonymousObjectCreationExpression boundAnonymousObjectCreationExpression)
        {
            Lazy<ImmutableArray<IOperation>> memberInitializers = new Lazy<ImmutableArray<IOperation>>(() => GetAnonymousObjectCreationInitializers(boundAnonymousObjectCreationExpression));
            SyntaxNode syntax = boundAnonymousObjectCreationExpression.Syntax;
            ITypeSymbol type = boundAnonymousObjectCreationExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundAnonymousObjectCreationExpression.ConstantValue);
            return new LazyAnonymousObjectCreationExpression(memberInitializers, syntax, type, constantValue);
        }

        private IPropertyReferenceExpression CreateBoundAnonymousPropertyDeclarationOperation(BoundAnonymousPropertyDeclaration boundAnonymousPropertyDeclaration)
        {
            PropertySymbol property = boundAnonymousPropertyDeclaration.Property;
            Lazy<IOperation> instance = new Lazy<IOperation>(() => null);
            ISymbol member = boundAnonymousPropertyDeclaration.Property;
            Lazy<ImmutableArray<IArgument>> argumentsInEvaluationOrder = new Lazy<ImmutableArray<IArgument>>(() => ImmutableArray<IArgument>.Empty);
            SyntaxNode syntax = boundAnonymousPropertyDeclaration.Syntax;
            ITypeSymbol type = boundAnonymousPropertyDeclaration.Type;
            Optional<object> constantValue = ConvertToOptional(boundAnonymousPropertyDeclaration.ConstantValue);
            return new LazyPropertyReferenceExpression(property, instance, member, argumentsInEvaluationOrder, syntax, type, constantValue);
        }

        private IObjectCreationExpression CreateBoundObjectCreationExpressionOperation(BoundObjectCreationExpression boundObjectCreationExpression)
        {
            IMethodSymbol constructor = boundObjectCreationExpression.Constructor;
            Lazy<ImmutableArray<IOperation>> memberInitializers = new Lazy<ImmutableArray<IOperation>>(() => GetObjectCreationInitializers(boundObjectCreationExpression));
            Lazy<ImmutableArray<IArgument>> argumentsInEvaluationOrder = new Lazy<ImmutableArray<IArgument>>(() =>
                DeriveArguments(boundObjectCreationExpression,
                                boundObjectCreationExpression.BinderOpt,
                                boundObjectCreationExpression.Constructor,
                                boundObjectCreationExpression.Constructor,
                                boundObjectCreationExpression.Arguments,
                                boundObjectCreationExpression.ArgumentNamesOpt,
                                boundObjectCreationExpression.ArgsToParamsOpt,
                                boundObjectCreationExpression.ArgumentRefKindsOpt,
                                boundObjectCreationExpression.Constructor.Parameters,
                                boundObjectCreationExpression.Expanded,
                                boundObjectCreationExpression.Syntax));
            SyntaxNode syntax = boundObjectCreationExpression.Syntax;
            ITypeSymbol type = boundObjectCreationExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundObjectCreationExpression.ConstantValue);
            return new LazyObjectCreationExpression(constructor, memberInitializers, argumentsInEvaluationOrder, syntax, type, constantValue);
        }

        private IOperation CreateUnboundLambdaOperation(UnboundLambda unboundLambda)
        {
            // We want to ensure that we never see the UnboundLambda node, and that we don't end up having two different IOperation
            // nodes for the lambda expression. So, we ask the semantic model for the IOperation node for the unbound lambda syntax.
            // We are counting on the fact that will do the error recovery and actually create the BoundLambda node appropriate for
            // this syntax node.
            var lambdaOperation = _semanticModel.GetOperationInternal(unboundLambda.Syntax);
            Debug.Assert(lambdaOperation.Kind == OperationKind.LambdaExpression);
            return lambdaOperation;
        }

        private ILambdaExpression CreateBoundLambdaOperation(BoundLambda boundLambda)
        {
            IMethodSymbol signature = boundLambda.Symbol;
            Lazy<IBlockStatement> body = new Lazy<IBlockStatement>(() => (IBlockStatement)Create(boundLambda.Body));
            SyntaxNode syntax = boundLambda.Syntax;
            // This matches the SemanticModel implementation. This is because in VB, lambdas by themselves
            // do not have a type. To get the type of a lambda expression in the SemanticModel, you need to look at
            // TypeInfo.ConvertedType, rather than TypeInfo.Type. We replicate that behavior here. To get the type of
            // an ILambdaExpression, you need to look at the parent IConversionExpression.
            ITypeSymbol type = null;
            Optional<object> constantValue = ConvertToOptional(boundLambda.ConstantValue);
            return new LazyLambdaExpression(signature, body, syntax, type, constantValue);
        }

        private ILocalFunctionStatement CreateBoundLocalFunctionStatementOperation(BoundLocalFunctionStatement boundLocalFunctionStatement)
        {
            IMethodSymbol localFunctionSymbol = boundLocalFunctionStatement.Symbol;
            Lazy<IBlockStatement> body = new Lazy<IBlockStatement>(() => (IBlockStatement)Create(boundLocalFunctionStatement.Body));
            SyntaxNode syntax = boundLocalFunctionStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyLocalFunctionStatement(localFunctionSymbol, body, syntax, type, constantValue);
        }

        private IOperation CreateBoundConversionOperation(BoundConversion boundConversion)
        {
            ConversionKind conversionKind = GetConversionKind(boundConversion.ConversionKind);
            if (boundConversion.ConversionKind == CSharp.ConversionKind.MethodGroup)
            {
                IMethodSymbol method = boundConversion.SymbolOpt;
                bool isVirtual = method != null && (method.IsAbstract || method.IsOverride || method.IsVirtual) && !boundConversion.SuppressVirtualCalls;
                Lazy<IOperation> instance = new Lazy<IOperation>(() => Create((boundConversion.Operand as BoundMethodGroup)?.InstanceOpt));
                SyntaxNode syntax = boundConversion.Syntax;
                ITypeSymbol type = boundConversion.Type;
                Optional<object> constantValue = ConvertToOptional(boundConversion.ConstantValue);
                return new LazyMethodBindingExpression(method, isVirtual, instance, method, syntax, type, constantValue);
            }
            else
            {
                Lazy<IOperation> operand = new Lazy<IOperation>(() => Create(boundConversion.Operand));

                bool isExplicit = boundConversion.ExplicitCastInCode;
                bool usesOperatorMethod = boundConversion.ConversionKind == CSharp.ConversionKind.ExplicitUserDefined || boundConversion.ConversionKind == CSharp.ConversionKind.ImplicitUserDefined;
                IMethodSymbol operatorMethod = boundConversion.SymbolOpt;
                SyntaxNode syntax = boundConversion.Syntax;
                ITypeSymbol type = boundConversion.Type;
                Optional<object> constantValue = ConvertToOptional(boundConversion.ConstantValue);
                return new LazyConversionExpression(operand, conversionKind, isExplicit, usesOperatorMethod, operatorMethod, syntax, type, constantValue);
            }
        }

        private IConversionExpression CreateBoundAsOperatorOperation(BoundAsOperator boundAsOperator)
        {
            Lazy<IOperation> operand = new Lazy<IOperation>(() => Create(boundAsOperator.Operand));
            ConversionKind conversionKind = Semantics.ConversionKind.TryCast;
            bool isExplicit = true;
            bool usesOperatorMethod = false;
            IMethodSymbol operatorMethod = null;
            SyntaxNode syntax = boundAsOperator.Syntax;
            ITypeSymbol type = boundAsOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundAsOperator.ConstantValue);
            return new LazyConversionExpression(operand, conversionKind, isExplicit, usesOperatorMethod, operatorMethod, syntax, type, constantValue);
        }

        private IIsTypeExpression CreateBoundIsOperatorOperation(BoundIsOperator boundIsOperator)
        {
            Lazy<IOperation> operand = new Lazy<IOperation>(() => Create(boundIsOperator.Operand));
            ITypeSymbol isType = boundIsOperator.TargetType.Type;
            SyntaxNode syntax = boundIsOperator.Syntax;
            ITypeSymbol type = boundIsOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundIsOperator.ConstantValue);
            return new LazyIsTypeExpression(operand, isType, syntax, type, constantValue);
        }

        private ISizeOfExpression CreateBoundSizeOfOperatorOperation(BoundSizeOfOperator boundSizeOfOperator)
        {
            ITypeSymbol typeOperand = boundSizeOfOperator.SourceType.Type;
            SyntaxNode syntax = boundSizeOfOperator.Syntax;
            ITypeSymbol type = boundSizeOfOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundSizeOfOperator.ConstantValue);
            return new SizeOfExpression(typeOperand, syntax, type, constantValue);
        }

        private ITypeOfExpression CreateBoundTypeOfOperatorOperation(BoundTypeOfOperator boundTypeOfOperator)
        {
            ITypeSymbol typeOperand = boundTypeOfOperator.SourceType.Type;
            SyntaxNode syntax = boundTypeOfOperator.Syntax;
            ITypeSymbol type = boundTypeOfOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundTypeOfOperator.ConstantValue);
            return new TypeOfExpression(typeOperand, syntax, type, constantValue);
        }

        private IArrayCreationExpression CreateBoundArrayCreationOperation(BoundArrayCreation boundArrayCreation)
        {
            ITypeSymbol elementType = GetArrayCreationElementType(boundArrayCreation);
            Lazy<ImmutableArray<IOperation>> dimensionSizes = new Lazy<ImmutableArray<IOperation>>(() => boundArrayCreation.Bounds.SelectAsArray(n => Create(n)));
            Lazy<IArrayInitializer> initializer = new Lazy<IArrayInitializer>(() => (IArrayInitializer)Create(boundArrayCreation.InitializerOpt));
            SyntaxNode syntax = boundArrayCreation.Syntax;
            ITypeSymbol type = boundArrayCreation.Type;
            Optional<object> constantValue = ConvertToOptional(boundArrayCreation.ConstantValue);
            return new LazyArrayCreationExpression(elementType, dimensionSizes, initializer, syntax, type, constantValue);
        }

        private IArrayInitializer CreateBoundArrayInitializationOperation(BoundArrayInitialization boundArrayInitialization)
        {
            Lazy<ImmutableArray<IOperation>> elementValues = new Lazy<ImmutableArray<IOperation>>(() => boundArrayInitialization.Initializers.SelectAsArray(n => Create(n)));
            SyntaxNode syntax = boundArrayInitialization.Syntax;
            ITypeSymbol type = boundArrayInitialization.Type;
            Optional<object> constantValue = ConvertToOptional(boundArrayInitialization.ConstantValue);
            return new LazyArrayInitializer(elementValues, syntax, type, constantValue);
        }

        private IDefaultValueExpression CreateBoundDefaultExpressionOperation(BoundDefaultExpression boundDefaultExpression)
        {
            SyntaxNode syntax = boundDefaultExpression.Syntax;
            ITypeSymbol type = boundDefaultExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundDefaultExpression.ConstantValue);
            return new DefaultValueExpression(syntax, type, constantValue);
        }

        private IInstanceReferenceExpression CreateBoundBaseReferenceOperation(BoundBaseReference boundBaseReference)
        {
            InstanceReferenceKind instanceReferenceKind = InstanceReferenceKind.BaseClass;
            SyntaxNode syntax = boundBaseReference.Syntax;
            ITypeSymbol type = boundBaseReference.Type;
            Optional<object> constantValue = ConvertToOptional(boundBaseReference.ConstantValue);
            return new InstanceReferenceExpression(instanceReferenceKind, syntax, type, constantValue);
        }

        private IInstanceReferenceExpression CreateBoundThisReferenceOperation(BoundThisReference boundThisReference)
        {
            InstanceReferenceKind instanceReferenceKind = boundThisReference.Syntax.Kind() == SyntaxKind.ThisExpression ? InstanceReferenceKind.Explicit : InstanceReferenceKind.Implicit;
            SyntaxNode syntax = boundThisReference.Syntax;
            ITypeSymbol type = boundThisReference.Type;
            Optional<object> constantValue = ConvertToOptional(boundThisReference.ConstantValue);
            return new InstanceReferenceExpression(instanceReferenceKind, syntax, type, constantValue);
        }

        private ISimpleAssignmentExpression CreateBoundAssignmentOperatorOperation(BoundAssignmentOperator boundAssignmentOperator)
        {
            Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundAssignmentOperator.Left));
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundAssignmentOperator.Right));
            SyntaxNode syntax = boundAssignmentOperator.Syntax;
            ITypeSymbol type = boundAssignmentOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundAssignmentOperator.ConstantValue);
            return new LazySimpleAssignmentExpression(target, value, syntax, type, constantValue);
        }

        private ICompoundAssignmentExpression CreateBoundCompoundAssignmentOperatorOperation(BoundCompoundAssignmentOperator boundCompoundAssignmentOperator)
        {
            BinaryOperationKind binaryOperationKind = Helper.DeriveBinaryOperationKind(boundCompoundAssignmentOperator.Operator.Kind);
            Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundCompoundAssignmentOperator.Left));
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundCompoundAssignmentOperator.Right));
            bool usesOperatorMethod = (boundCompoundAssignmentOperator.Operator.Kind & BinaryOperatorKind.TypeMask) == BinaryOperatorKind.UserDefined;
            IMethodSymbol operatorMethod = boundCompoundAssignmentOperator.Operator.Method;
            SyntaxNode syntax = boundCompoundAssignmentOperator.Syntax;
            ITypeSymbol type = boundCompoundAssignmentOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundCompoundAssignmentOperator.ConstantValue);
            return new LazyCompoundAssignmentExpression(binaryOperationKind, target, value, usesOperatorMethod, operatorMethod, syntax, type, constantValue);
        }

        private IIncrementExpression CreateBoundIncrementOperatorOperation(BoundIncrementOperator boundIncrementOperator)
        {
            UnaryOperationKind incrementOperationKind = Helper.DeriveUnaryOperationKind(boundIncrementOperator.OperatorKind);
            Lazy<IOperation> target = new Lazy<IOperation>(() => Create(boundIncrementOperator.Operand));
            bool usesOperatorMethod = (boundIncrementOperator.OperatorKind & UnaryOperatorKind.TypeMask) == UnaryOperatorKind.UserDefined;
            IMethodSymbol operatorMethod = boundIncrementOperator.MethodOpt;
            SyntaxNode syntax = boundIncrementOperator.Syntax;
            ITypeSymbol type = boundIncrementOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundIncrementOperator.ConstantValue);
            return new LazyIncrementExpression(incrementOperationKind, target, usesOperatorMethod, operatorMethod, syntax, type, constantValue);
        }

        private IInvalidExpression CreateBoundBadExpressionOperation(BoundBadExpression boundBadExpression)
        {
            Lazy<ImmutableArray<IOperation>> children = new Lazy<ImmutableArray<IOperation>>(() => boundBadExpression.ChildBoundNodes.SelectAsArray(n => Create(n)));
            SyntaxNode syntax = boundBadExpression.Syntax;
            ITypeSymbol type = boundBadExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundBadExpression.ConstantValue);
            return new LazyInvalidExpression(children, syntax, type, constantValue);
        }

        private ITypeParameterObjectCreationExpression CreateBoundNewTOperation(BoundNewT boundNewT)
        {
            SyntaxNode syntax = boundNewT.Syntax;
            ITypeSymbol type = boundNewT.Type;
            Optional<object> constantValue = ConvertToOptional(boundNewT.ConstantValue);
            return new TypeParameterObjectCreationExpression(syntax, type, constantValue);
        }

        private IUnaryOperatorExpression CreateBoundUnaryOperatorOperation(BoundUnaryOperator boundUnaryOperator)
        {
            UnaryOperationKind unaryOperationKind = Helper.DeriveUnaryOperationKind(boundUnaryOperator.OperatorKind);
            Lazy<IOperation> operand = new Lazy<IOperation>(() => Create(boundUnaryOperator.Operand));
            bool usesOperatorMethod = (boundUnaryOperator.OperatorKind & UnaryOperatorKind.TypeMask) == UnaryOperatorKind.UserDefined;
            IMethodSymbol operatorMethod = boundUnaryOperator.MethodOpt;
            SyntaxNode syntax = boundUnaryOperator.Syntax;
            ITypeSymbol type = boundUnaryOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundUnaryOperator.ConstantValue);
            return new LazyUnaryOperatorExpression(unaryOperationKind, operand, usesOperatorMethod, operatorMethod, syntax, type, constantValue);
        }

        private IBinaryOperatorExpression CreateBoundBinaryOperatorOperation(BoundBinaryOperator boundBinaryOperator)
        {
            BinaryOperationKind binaryOperationKind = Helper.DeriveBinaryOperationKind(boundBinaryOperator.OperatorKind);
            Lazy<IOperation> leftOperand = new Lazy<IOperation>(() => Create(boundBinaryOperator.Left));
            Lazy<IOperation> rightOperand = new Lazy<IOperation>(() => Create(boundBinaryOperator.Right));
            bool usesOperatorMethod = (boundBinaryOperator.OperatorKind & BinaryOperatorKind.TypeMask) == BinaryOperatorKind.UserDefined;
            IMethodSymbol operatorMethod = boundBinaryOperator.MethodOpt;
            SyntaxNode syntax = boundBinaryOperator.Syntax;
            ITypeSymbol type = boundBinaryOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundBinaryOperator.ConstantValue);
            return new LazyBinaryOperatorExpression(binaryOperationKind, leftOperand, rightOperand, usesOperatorMethod, operatorMethod, syntax, type, constantValue);
        }

        private IConditionalChoiceExpression CreateBoundConditionalOperatorOperation(BoundConditionalOperator boundConditionalOperator)
        {
            Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundConditionalOperator.Condition));
            Lazy<IOperation> ifTrueValue = new Lazy<IOperation>(() => Create(boundConditionalOperator.Consequence));
            Lazy<IOperation> ifFalseValue = new Lazy<IOperation>(() => Create(boundConditionalOperator.Alternative));
            SyntaxNode syntax = boundConditionalOperator.Syntax;
            ITypeSymbol type = boundConditionalOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundConditionalOperator.ConstantValue);
            return new LazyConditionalChoiceExpression(condition, ifTrueValue, ifFalseValue, syntax, type, constantValue);
        }

        private INullCoalescingExpression CreateBoundNullCoalescingOperatorOperation(BoundNullCoalescingOperator boundNullCoalescingOperator)
        {
            Lazy<IOperation> primaryOperand = new Lazy<IOperation>(() => Create(boundNullCoalescingOperator.LeftOperand));
            Lazy<IOperation> secondaryOperand = new Lazy<IOperation>(() => Create(boundNullCoalescingOperator.RightOperand));
            SyntaxNode syntax = boundNullCoalescingOperator.Syntax;
            ITypeSymbol type = boundNullCoalescingOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundNullCoalescingOperator.ConstantValue);
            return new LazyNullCoalescingExpression(primaryOperand, secondaryOperand, syntax, type, constantValue);
        }

        private IAwaitExpression CreateBoundAwaitExpressionOperation(BoundAwaitExpression boundAwaitExpression)
        {
            Lazy<IOperation> awaitedValue = new Lazy<IOperation>(() => Create(boundAwaitExpression.Expression));
            SyntaxNode syntax = boundAwaitExpression.Syntax;
            ITypeSymbol type = boundAwaitExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundAwaitExpression.ConstantValue);
            return new LazyAwaitExpression(awaitedValue, syntax, type, constantValue);
        }

        private IArrayElementReferenceExpression CreateBoundArrayAccessOperation(BoundArrayAccess boundArrayAccess)
        {
            Lazy<IOperation> arrayReference = new Lazy<IOperation>(() => Create(boundArrayAccess.Expression));
            Lazy<ImmutableArray<IOperation>> indices = new Lazy<ImmutableArray<IOperation>>(() => boundArrayAccess.Indices.SelectAsArray(n => Create(n)));
            SyntaxNode syntax = boundArrayAccess.Syntax;
            ITypeSymbol type = boundArrayAccess.Type;
            Optional<object> constantValue = ConvertToOptional(boundArrayAccess.ConstantValue);
            return new LazyArrayElementReferenceExpression(arrayReference, indices, syntax, type, constantValue);
        }

        private IPointerIndirectionReferenceExpression CreateBoundPointerIndirectionOperatorOperation(BoundPointerIndirectionOperator boundPointerIndirectionOperator)
        {
            Lazy<IOperation> pointer = new Lazy<IOperation>(() => Create(boundPointerIndirectionOperator.Operand));
            SyntaxNode syntax = boundPointerIndirectionOperator.Syntax;
            ITypeSymbol type = boundPointerIndirectionOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundPointerIndirectionOperator.ConstantValue);
            return new LazyPointerIndirectionReferenceExpression(pointer, syntax, type, constantValue);
        }

        private INameOfExpression CreateBoundNameOfOperatorOperation(BoundNameOfOperator boundNameOfOperator)
        {
            Lazy<IOperation> argument = new Lazy<IOperation>(() => Create(boundNameOfOperator.Argument));
            SyntaxNode syntax = boundNameOfOperator.Syntax;
            ITypeSymbol type = boundNameOfOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundNameOfOperator.ConstantValue);
            return new LazyNameOfExpression(argument, syntax, type, constantValue);
        }

        private IThrowExpression CreateBoundThrowExpressionOperation(BoundThrowExpression boundThrowExpression)
        {
            Lazy<IOperation> expression = new Lazy<IOperation>(() => Create(boundThrowExpression.Expression));
            SyntaxNode syntax = boundThrowExpression.Syntax;
            ITypeSymbol type = boundThrowExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundThrowExpression.ConstantValue);
            return new LazyThrowExpression(expression, syntax, type, constantValue);
        }

        private IAddressOfExpression CreateBoundAddressOfOperatorOperation(BoundAddressOfOperator boundAddressOfOperator)
        {
            Lazy<IOperation> reference = new Lazy<IOperation>(() => Create(boundAddressOfOperator.Operand));
            SyntaxNode syntax = boundAddressOfOperator.Syntax;
            ITypeSymbol type = boundAddressOfOperator.Type;
            Optional<object> constantValue = ConvertToOptional(boundAddressOfOperator.ConstantValue);
            return new LazyAddressOfExpression(reference, syntax, type, constantValue);
        }

        private IInstanceReferenceExpression CreateBoundImplicitReceiverOperation(BoundImplicitReceiver boundImplicitReceiver)
        {
            InstanceReferenceKind instanceReferenceKind = InstanceReferenceKind.Implicit;
            SyntaxNode syntax = boundImplicitReceiver.Syntax;
            ITypeSymbol type = boundImplicitReceiver.Type;
            Optional<object> constantValue = ConvertToOptional(boundImplicitReceiver.ConstantValue);
            return new InstanceReferenceExpression(instanceReferenceKind, syntax, type, constantValue);
        }

        private IConditionalAccessExpression CreateBoundConditionalAccessOperation(BoundConditionalAccess boundConditionalAccess)
        {
            Lazy<IOperation> conditionalValue = new Lazy<IOperation>(() => Create(boundConditionalAccess.AccessExpression));
            Lazy<IOperation> conditionalInstance = new Lazy<IOperation>(() => Create(boundConditionalAccess.Receiver));
            SyntaxNode syntax = boundConditionalAccess.Syntax;
            ITypeSymbol type = boundConditionalAccess.Type;
            Optional<object> constantValue = ConvertToOptional(boundConditionalAccess.ConstantValue);
            return new LazyConditionalAccessExpression(conditionalValue, conditionalInstance, syntax, type, constantValue);
        }

        private IConditionalAccessInstanceExpression CreateBoundConditionalReceiverOperation(BoundConditionalReceiver boundConditionalReceiver)
        {
            SyntaxNode syntax = boundConditionalReceiver.Syntax;
            ITypeSymbol type = boundConditionalReceiver.Type;
            Optional<object> constantValue = ConvertToOptional(boundConditionalReceiver.ConstantValue);
            return new ConditionalAccessInstanceExpression(syntax, type, constantValue);
        }

        private IFieldInitializer CreateBoundFieldEqualsValueOperation(BoundFieldEqualsValue boundFieldEqualsValue)
        {
            ImmutableArray<IFieldSymbol> initializedFields = ImmutableArray.Create<IFieldSymbol>(boundFieldEqualsValue.Field);
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundFieldEqualsValue.Value));
            OperationKind kind = OperationKind.FieldInitializerAtDeclaration;
            SyntaxNode syntax = boundFieldEqualsValue.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyFieldInitializer(initializedFields, value, kind, syntax, type, constantValue);
        }

        private IPropertyInitializer CreateBoundPropertyEqualsValueOperation(BoundPropertyEqualsValue boundPropertyEqualsValue)
        {
            IPropertySymbol initializedProperty = boundPropertyEqualsValue.Property;
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundPropertyEqualsValue.Value));
            OperationKind kind = OperationKind.PropertyInitializerAtDeclaration;
            SyntaxNode syntax = boundPropertyEqualsValue.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyPropertyInitializer(initializedProperty, value, kind, syntax, type, constantValue);
        }

        private IParameterInitializer CreateBoundParameterEqualsValueOperation(BoundParameterEqualsValue boundParameterEqualsValue)
        {
            IParameterSymbol parameter = boundParameterEqualsValue.Parameter;
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundParameterEqualsValue.Value));
            OperationKind kind = OperationKind.ParameterInitializerAtDeclaration;
            SyntaxNode syntax = boundParameterEqualsValue.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyParameterInitializer(parameter, value, kind, syntax, type, constantValue);
        }

        private IBlockStatement CreateBoundBlockOperation(BoundBlock boundBlock)
        {
            Lazy<ImmutableArray<IOperation>> statements =
                new Lazy<ImmutableArray<IOperation>>(() => boundBlock.Statements.Select(s => Create(s)).Where(s => s.Kind != OperationKind.None).ToImmutableArray());

            ImmutableArray<ILocalSymbol> locals = boundBlock.Locals.As<ILocalSymbol>();
            SyntaxNode syntax = boundBlock.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyBlockStatement(statements, locals, syntax, type, constantValue);
        }

        private IBranchStatement CreateBoundContinueStatementOperation(BoundContinueStatement boundContinueStatement)
        {
            ILabelSymbol target = boundContinueStatement.Label;
            BranchKind branchKind = BranchKind.Continue;
            SyntaxNode syntax = boundContinueStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new BranchStatement(target, branchKind, syntax, type, constantValue);
        }

        private IBranchStatement CreateBoundBreakStatementOperation(BoundBreakStatement boundBreakStatement)
        {
            ILabelSymbol target = boundBreakStatement.Label;
            BranchKind branchKind = BranchKind.Break;
            SyntaxNode syntax = boundBreakStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new BranchStatement(target, branchKind, syntax, type, constantValue);
        }

        private IReturnStatement CreateBoundYieldBreakStatementOperation(BoundYieldBreakStatement boundYieldBreakStatement)
        {
            Lazy<IOperation> returnedValue = new Lazy<IOperation>(() => Create(null));
            SyntaxNode syntax = boundYieldBreakStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyReturnStatement(OperationKind.YieldBreakStatement, returnedValue, syntax, type, constantValue);
        }

        private IBranchStatement CreateBoundGotoStatementOperation(BoundGotoStatement boundGotoStatement)
        {
            ILabelSymbol target = boundGotoStatement.Label;
            BranchKind branchKind = BranchKind.GoTo;
            SyntaxNode syntax = boundGotoStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new BranchStatement(target, branchKind, syntax, type, constantValue);
        }

        private IEmptyStatement CreateBoundNoOpStatementOperation(BoundNoOpStatement boundNoOpStatement)
        {
            SyntaxNode syntax = boundNoOpStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new EmptyStatement(syntax, type, constantValue);
        }

        private IIfStatement CreateBoundIfStatementOperation(BoundIfStatement boundIfStatement)
        {
            Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundIfStatement.Condition));
            Lazy<IOperation> ifTrueStatement = new Lazy<IOperation>(() => Create(boundIfStatement.Consequence));
            Lazy<IOperation> ifFalseStatement = new Lazy<IOperation>(() => Create(boundIfStatement.AlternativeOpt));
            SyntaxNode syntax = boundIfStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyIfStatement(condition, ifTrueStatement, ifFalseStatement, syntax, type, constantValue);
        }

        private IWhileUntilLoopStatement CreateBoundWhileStatementOperation(BoundWhileStatement boundWhileStatement)
        {
            bool isTopTest = true;
            bool isWhile = true;
            Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundWhileStatement.Condition));
            LoopKind loopKind = LoopKind.WhileUntil;
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundWhileStatement.Body));
            SyntaxNode syntax = boundWhileStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyWhileUntilLoopStatement(isTopTest, isWhile, condition, loopKind, body, syntax, type, constantValue);
        }

        private IWhileUntilLoopStatement CreateBoundDoStatementOperation(BoundDoStatement boundDoStatement)
        {
            bool isTopTest = false;
            bool isWhile = true;
            Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundDoStatement.Condition));
            LoopKind loopKind = LoopKind.WhileUntil;
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundDoStatement.Body));
            SyntaxNode syntax = boundDoStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyWhileUntilLoopStatement(isTopTest, isWhile, condition, loopKind, body, syntax, type, constantValue);
        }

        private IForLoopStatement CreateBoundForStatementOperation(BoundForStatement boundForStatement)
        {
            Lazy<ImmutableArray<IOperation>> before = new Lazy<ImmutableArray<IOperation>>(() => ToStatements(boundForStatement.Initializer));
            Lazy<ImmutableArray<IOperation>> atLoopBottom = new Lazy<ImmutableArray<IOperation>>(() => ToStatements(boundForStatement.Increment));
            ImmutableArray<ILocalSymbol> locals = boundForStatement.OuterLocals.As<ILocalSymbol>();
            Lazy<IOperation> condition = new Lazy<IOperation>(() => Create(boundForStatement.Condition));
            LoopKind loopKind = LoopKind.For;
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundForStatement.Body));
            SyntaxNode syntax = boundForStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyForLoopStatement(before, atLoopBottom, locals, condition, loopKind, body, syntax, type, constantValue);
        }

        private IForEachLoopStatement CreateBoundForEachStatementOperation(BoundForEachStatement boundForEachStatement)
        {
            ILocalSymbol iterationVariable = boundForEachStatement.IterationVariables.Length == 1 ?
                                                                                    boundForEachStatement.IterationVariables.FirstOrDefault() :
                                                                                    null;
            Lazy<IOperation> collection = new Lazy<IOperation>(() => Create(boundForEachStatement.Expression));
            LoopKind loopKind = LoopKind.ForEach;
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundForEachStatement.Body));
            SyntaxNode syntax = boundForEachStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyForEachLoopStatement(iterationVariable, collection, loopKind, body, syntax, type, constantValue);
        }

        private ISwitchStatement CreateBoundSwitchStatementOperation(BoundSwitchStatement boundSwitchStatement)
        {
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundSwitchStatement.Expression));
            Lazy<ImmutableArray<ISwitchCase>> cases = new Lazy<ImmutableArray<ISwitchCase>>(() => GetSwitchStatementCases(boundSwitchStatement));
            SyntaxNode syntax = boundSwitchStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazySwitchStatement(value, cases, syntax, type, constantValue);
        }

        private ICaseClause CreateBoundSwitchLabelOperation(BoundSwitchLabel boundSwitchLabel)
        {
            SyntaxNode syntax = boundSwitchLabel.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);

            if (boundSwitchLabel.ExpressionOpt != null)
            {
                Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundSwitchLabel.ExpressionOpt));
                BinaryOperationKind equality = GetLabelEqualityKind(boundSwitchLabel);
                return new LazySingleValueCaseClause(value, equality, CaseKind.SingleValue, syntax, type, constantValue);
            }
            else
            {
                return new DefaultCaseClause(syntax, type, constantValue);
            }
        }

        private ITryStatement CreateBoundTryStatementOperation(BoundTryStatement boundTryStatement)
        {
            Lazy<IBlockStatement> body = new Lazy<IBlockStatement>(() => (IBlockStatement)Create(boundTryStatement.TryBlock));
            Lazy<ImmutableArray<ICatchClause>> catches = new Lazy<ImmutableArray<ICatchClause>>(() => boundTryStatement.CatchBlocks.SelectAsArray(n => (ICatchClause)Create(n)));
            Lazy<IBlockStatement> finallyHandler = new Lazy<IBlockStatement>(() => (IBlockStatement)Create(boundTryStatement.FinallyBlockOpt));
            SyntaxNode syntax = boundTryStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyTryStatement(body, catches, finallyHandler, syntax, type, constantValue);
        }

        private ICatchClause CreateBoundCatchBlockOperation(BoundCatchBlock boundCatchBlock)
        {
            Lazy<IBlockStatement> handler = new Lazy<IBlockStatement>(() => (IBlockStatement)Create(boundCatchBlock.Body));
            ITypeSymbol caughtType = boundCatchBlock.ExceptionTypeOpt;
            Lazy<IOperation> filter = new Lazy<IOperation>(() => Create(boundCatchBlock.ExceptionFilterOpt));
            ILocalSymbol exceptionLocal = (boundCatchBlock.Locals.FirstOrDefault()?.DeclarationKind == CSharp.Symbols.LocalDeclarationKind.CatchVariable) ? boundCatchBlock.Locals.FirstOrDefault() : null;
            SyntaxNode syntax = boundCatchBlock.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyCatchClause(handler, caughtType, filter, exceptionLocal, syntax, type, constantValue);
        }

        private IFixedStatement CreateBoundFixedStatementOperation(BoundFixedStatement boundFixedStatement)
        {
            Lazy<IVariableDeclarationStatement> variables = new Lazy<IVariableDeclarationStatement>(() => (IVariableDeclarationStatement)Create(boundFixedStatement.Declarations));
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundFixedStatement.Body));
            SyntaxNode syntax = boundFixedStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyFixedStatement(variables, body, syntax, type, constantValue);
        }

        private IUsingStatement CreateBoundUsingStatementOperation(BoundUsingStatement boundUsingStatement)
        {
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundUsingStatement.Body));
            Lazy<IVariableDeclarationStatement> declaration = new Lazy<IVariableDeclarationStatement>(() => (IVariableDeclarationStatement)Create(boundUsingStatement.DeclarationsOpt));
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundUsingStatement.ExpressionOpt));
            SyntaxNode syntax = boundUsingStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyUsingStatement(body, declaration, value, syntax, type, constantValue);
        }

        private IThrowStatement CreateBoundThrowStatementOperation(BoundThrowStatement boundThrowStatement)
        {
            Lazy<IOperation> thrownObject = new Lazy<IOperation>(() => Create(boundThrowStatement.ExpressionOpt));
            SyntaxNode syntax = boundThrowStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyThrowStatement(thrownObject, syntax, type, constantValue);
        }

        private IReturnStatement CreateBoundReturnStatementOperation(BoundReturnStatement boundReturnStatement)
        {
            Lazy<IOperation> returnedValue = new Lazy<IOperation>(() => Create(boundReturnStatement.ExpressionOpt));
            SyntaxNode syntax = boundReturnStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyReturnStatement(OperationKind.ReturnStatement, returnedValue, syntax, type, constantValue);
        }

        private IReturnStatement CreateBoundYieldReturnStatementOperation(BoundYieldReturnStatement boundYieldReturnStatement)
        {
            Lazy<IOperation> returnedValue = new Lazy<IOperation>(() => Create(boundYieldReturnStatement.Expression));
            SyntaxNode syntax = boundYieldReturnStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyReturnStatement(OperationKind.YieldReturnStatement, returnedValue, syntax, type, constantValue);
        }

        private ILockStatement CreateBoundLockStatementOperation(BoundLockStatement boundLockStatement)
        {
            Lazy<IOperation> lockedObject = new Lazy<IOperation>(() => Create(boundLockStatement.Argument));
            Lazy<IOperation> body = new Lazy<IOperation>(() => Create(boundLockStatement.Body));
            SyntaxNode syntax = boundLockStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyLockStatement(lockedObject, body, syntax, type, constantValue);
        }

        private IInvalidStatement CreateBoundBadStatementOperation(BoundBadStatement boundBadStatement)
        {
            Lazy<ImmutableArray<IOperation>> children = new Lazy<ImmutableArray<IOperation>>(() => boundBadStatement.ChildBoundNodes.SelectAsArray(n => Create(n)));
            SyntaxNode syntax = boundBadStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyInvalidStatement(children, syntax, type, constantValue);
        }

        private IVariableDeclarationStatement CreateBoundLocalDeclarationOperation(BoundLocalDeclaration boundLocalDeclaration)
        {
            Lazy<ImmutableArray<IVariableDeclaration>> declarations = new Lazy<ImmutableArray<IVariableDeclaration>>(() =>
                ImmutableArray.Create(OperationFactory.CreateVariableDeclaration(boundLocalDeclaration.LocalSymbol, Create(boundLocalDeclaration.InitializerOpt), boundLocalDeclaration.Syntax)));
            SyntaxNode syntax = boundLocalDeclaration.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyVariableDeclarationStatement(declarations, syntax, type, constantValue);
        }

        private IVariableDeclarationStatement CreateBoundMultipleLocalDeclarationsOperation(BoundMultipleLocalDeclarations boundMultipleLocalDeclarations)
        {
            Lazy<ImmutableArray<IVariableDeclaration>> declarations = new Lazy<ImmutableArray<IVariableDeclaration>>(() =>
                boundMultipleLocalDeclarations.LocalDeclarations.SelectAsArray(declaration =>
                    OperationFactory.CreateVariableDeclaration(declaration.LocalSymbol, Create(declaration.InitializerOpt), declaration.Syntax)));
            SyntaxNode syntax = boundMultipleLocalDeclarations.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyVariableDeclarationStatement(declarations, syntax, type, constantValue);
        }

        private ILabelStatement CreateBoundLabelStatementOperation(BoundLabelStatement boundLabelStatement)
        {
            ILabelSymbol label = boundLabelStatement.Label;
            Lazy<IOperation> labeledStatement = new Lazy<IOperation>(() => Create(null));
            SyntaxNode syntax = boundLabelStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyLabelStatement(label, labeledStatement, syntax, type, constantValue);
        }

        private ILabelStatement CreateBoundLabeledStatementOperation(BoundLabeledStatement boundLabeledStatement)
        {
            ILabelSymbol label = boundLabeledStatement.Label;
            Lazy<IOperation> labeledStatement = new Lazy<IOperation>(() => Create(boundLabeledStatement.Body));
            SyntaxNode syntax = boundLabeledStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyLabelStatement(label, labeledStatement, syntax, type, constantValue);
        }

        private IExpressionStatement CreateBoundExpressionStatementOperation(BoundExpressionStatement boundExpressionStatement)
        {
            Lazy<IOperation> expression = new Lazy<IOperation>(() => Create(boundExpressionStatement.Expression));
            SyntaxNode syntax = boundExpressionStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyExpressionStatement(expression, syntax, type, constantValue);
        }

        private IInterpolatedStringExpression CreateBoundInterpolatedStringExpressionOperation(BoundInterpolatedString boundInterpolatedString)
        {
            Lazy<ImmutableArray<IInterpolatedStringContent>> parts = new Lazy<ImmutableArray<IInterpolatedStringContent>>(() =>
                boundInterpolatedString.Parts.SelectAsArray(interpolatedStringContent => CreateBoundInterpolatedStringContentOperation(interpolatedStringContent)));
            SyntaxNode syntax = boundInterpolatedString.Syntax;
            ITypeSymbol type = boundInterpolatedString.Type;
            Optional<object> constantValue = ConvertToOptional(boundInterpolatedString.ConstantValue);
            return new LazyInterpolatedStringExpression(parts, syntax, type, constantValue);
        }

        private IInterpolatedStringContent CreateBoundInterpolatedStringContentOperation(BoundNode boundNode)
        {
            if (boundNode.Kind == BoundKind.StringInsert)
            {
                return (IInterpolatedStringContent)Create(boundNode);
            }
            else
            {
                return CreateBoundInterpolatedStringTextOperation(boundNode);
            }
        }

        private IInterpolation CreateBoundInterpolationOperation(BoundStringInsert boundStringInsert)
        {
            Lazy<IOperation> expression = new Lazy<IOperation>(() => Create(boundStringInsert.Value));
            Lazy<IOperation> alignment = new Lazy<IOperation>(() => Create(boundStringInsert.Alignment));
            Lazy<IOperation> format = new Lazy<IOperation>(() => Create(boundStringInsert.Format));
            SyntaxNode syntax = boundStringInsert.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyInterpolation(expression, alignment, format, syntax, type, constantValue);
        }

        private IInterpolatedStringText CreateBoundInterpolatedStringTextOperation(BoundNode boundNode)
        {
            Lazy<IOperation> text = new Lazy<IOperation>(() => Create(boundNode));
            SyntaxNode syntax = boundNode.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyInterpolatedStringText(text, syntax, type, constantValue);
        }

        private IConstantPattern CreateBoundConstantPatternOperation(BoundConstantPattern boundConstantPattern)
        {
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundConstantPattern.Value));
            SyntaxNode syntax = boundConstantPattern.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazyConstantPattern(value, syntax, type, constantValue);
        }

        private IDeclarationPattern CreateBoundDeclarationPatternOperation(BoundDeclarationPattern boundDeclarationPattern)
        {
            ISymbol variable = boundDeclarationPattern.Variable;
            SyntaxNode syntax = boundDeclarationPattern.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new DeclarationPattern(variable, syntax, type, constantValue);
        }

        private ISwitchStatement CreateBoundPatternSwitchStatementOperation(BoundPatternSwitchStatement boundPatternSwitchStatement)
        {
            Lazy<IOperation> value = new Lazy<IOperation>(() => Create(boundPatternSwitchStatement.Expression));
            Lazy<ImmutableArray<ISwitchCase>> cases = new Lazy<ImmutableArray<ISwitchCase>>(() => GetPatternSwitchStatementCases(boundPatternSwitchStatement));
            SyntaxNode syntax = boundPatternSwitchStatement.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);
            return new LazySwitchStatement(value, cases, syntax, type, constantValue);
        }

        private ICaseClause CreateBoundPatternSwitchLabelOperation(BoundPatternSwitchLabel boundPatternSwitchLabel)
        {
            SyntaxNode syntax = boundPatternSwitchLabel.Syntax;
            ITypeSymbol type = null;
            Optional<object> constantValue = default(Optional<object>);

            if (boundPatternSwitchLabel.Pattern.Kind == BoundKind.WildcardPattern)
            {
                // Default switch label in pattern switch statement is represented as a default case clause.
                return new DefaultCaseClause(syntax, type, constantValue);
            }
            else
            {
                LabelSymbol label = boundPatternSwitchLabel.Label;
                Lazy<IPattern> pattern = new Lazy<IPattern>(() => (IPattern)Create(boundPatternSwitchLabel.Pattern));
                Lazy<IOperation> guardExpression = new Lazy<IOperation>(() => Create(boundPatternSwitchLabel.Guard));
                return new LazyPatternCaseClause(label, pattern, guardExpression, syntax, type, constantValue);
            }
        }

        private IIsPatternExpression CreateBoundIsPatternExpressionOperation(BoundIsPatternExpression boundIsPatternExpression)
        {
            Lazy<IOperation> expression = new Lazy<IOperation>(() => Create(boundIsPatternExpression.Expression));
            Lazy<IPattern> pattern = new Lazy<IPattern>(() => (IPattern)Create(boundIsPatternExpression.Pattern));
            SyntaxNode syntax = boundIsPatternExpression.Syntax;
            ITypeSymbol type = boundIsPatternExpression.Type;
            Optional<object> constantValue = ConvertToOptional(boundIsPatternExpression.ConstantValue);
            return new LazyIsPatternExpression(expression, pattern, syntax, type, constantValue);
        }
    }
}