cp ${ProjectDir}${OutDir}*.dll $SolutionDir$OutDir
cp ${ProjectDir}${OutDir}*.pdb $SolutionDir$OutDir

# copy to test etc.

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Jobs.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Jobs.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\exe\Jhu.Graywulf.Scheduler.Server\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\exe\Jhu.Graywulf.Scheduler.Server\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Scheduler.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Scheduler.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.RemoteService.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.RemoteService.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Logging.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Logging.Test\${OutDir}

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.Auth\bin
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.Auth\bin

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.UI\bin
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\web\Jhu.Graywulf.Web.UI\bin

cp ${ProjectDir}${OutDir}*.dll ${SolutionDir}graywulf\test\Jhu.Graywulf.Web.Test\${OutDir}
cp ${ProjectDir}${OutDir}*.pdb ${SolutionDir}graywulf\test\Jhu.Graywulf.Web.Test\${OutDir}

