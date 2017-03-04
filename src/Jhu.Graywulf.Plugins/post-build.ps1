cp $ProjectDir$OutDir$TargetName.dll $SolutionDir$OutDir
cp $ProjectDir$OutDir$TargetName.pdb $SolutionDir$OutDir

# copy to test

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Jobs.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Jobs.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Scheduler.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Scheduler.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.Auth\bin
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.Auth\bin

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.UI\bin
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.UI\bin

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Web.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Web.Test\${OutDir}