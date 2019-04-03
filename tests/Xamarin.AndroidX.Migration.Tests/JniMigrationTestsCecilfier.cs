using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests
{
	public class JniMigrationTestsCecilfier : BaseTests
	{
		[Theory]
		[InlineData("", "")]
		[InlineData("()V", "()V")]
		[InlineData
            (
			    "(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
			    "(Landroid/content/Context;)Landroidx/fragment/app/Fragment;"
            )
        ]
		[InlineData
            (
			    "CreateFragment.(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
			    "CreateFragment.(Landroid/content/Context;)Landroidx/fragment/app/Fragment;"
            )
        ]
		[InlineData
            (
			    "(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
			    "(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;"
            )
        ]
		[InlineData
            (
			    "CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
			    "CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;"
            )
        ]
		[InlineData
            (
			    "(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
			    "(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V"
            )
        ]
		[InlineData
            (
			    "UpdateFragment.(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
			    "UpdateFragment.(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V"
            )
        ]
		[InlineData
            (
			    "(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
			    "(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V"
            )
        ]
		[InlineData
            (
			    "UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
			    "UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V"
            )
        ]
		[InlineData
            (
			    //mc++ "([Landroid/support/v4/graphics/PathParser;[Landroid/support/v4/graphics/PathParser;)V",
			    //mc++ "([Landroidx/core/graphics/PathParser;[Landroidx/core/graphics/PathParser;)V"
			    "(Landroid/support/v4/graphics/PathParser;Landroid/support/v4/graphics/PathParser;)V",
			    "(Landroidx/core/graphics/PathParser;Landroidx/core/graphics/PathParser;)V"
            )
        ]
		[InlineData
            (
			    //mc++ "([Landroid/support/v4/graphics/PathParser$PathDataNode;[Landroid/support/v4/graphics/PathParser$PathDataNode;)V",
			    //mc++ "([Landroidx/core/graphics/PathParser$PathDataNode;[Landroidx/core/graphics/PathParser$PathDataNode;)V"
			    "(Landroid/support/v4/graphics/PathParser$PathDataNode;Landroid/support/v4/graphics/PathParser$PathDataNode;)V",
			    "(Landroidx/core/graphics/PathParser$PathDataNode;Landroidx/core/graphics/PathParser$PathDataNode;)V"
            )
            ]
		[InlineData
            (
			    //mc++ "([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;",
			    //mc++ "([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;"
			    "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;",
			    "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;"
            )
        ]
		[InlineData
            (
			    //mc++ "([Ljava/lang/Object;[[Landroid/support/v4/graphics/PathParser;)[Ljava/lang/Object;",
			    //mc++ "([Ljava/lang/Object;[[Landroidx/core/graphics/PathParser;)[Ljava/lang/Object;"
			    "(Ljava/lang/Object;Landroid/support/v4/graphics/PathParser;)Ljava/lang/Object;",
			    "(Ljava/lang/Object;Landroidx/core/graphics/PathParser;)Ljava/lang/Object;"
            )
        ]
		[InlineData
            (
			    "java/lang/Object",
			    "java/lang/Object"
            )
        ]
		[InlineData
            (
			    "android/support/v4/app/Fragment",
			    "androidx/fragment/app/Fragment"
            )
        ]
		[InlineData
            (
			    "android/support/v7/app/ActionBar$Tab",
			    "androidx/appcompat/app/ActionBar$Tab"
            )
        ]
		[InlineData
            (
			    "android/support/v7/app/ActionBarDrawerToggle$Delegate",
			    "androidx/appcompat/app/ActionBarDrawerToggle$Delegate"
            )
        ]
		[InlineData
            (
			    "android/support/v7/app/ActionBar$Tab$ThisDoesNotExist",
			    "androidx/appcompat/app/ActionBar$Tab$ThisDoesNotExist"
            )
        ]
		[InlineData
            (
			    "android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist",
			    "androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist"
            )
        ]
		[InlineData
            (
			    "android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis",
			    "androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis"
            )
        ]
		[InlineData
            (
			    "(I)Landroid/support/v7/app/AlertDialog$Builder;",
			    "(I)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
		[InlineData
            (
			    "(L;L;L;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "(L;L;L;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
		public void JniStringAreCorrectlyMapped(string supportJni, string androidxJni)
		{
            AndroidXMigrator migrator = null;
            migrator = new AndroidXMigrator(null, null);
            string mapped = migrator.ReplaceJniSignatureRedth(supportJni);

			Assert.Equal(androidxJni, mapped);
		}

	}
}
