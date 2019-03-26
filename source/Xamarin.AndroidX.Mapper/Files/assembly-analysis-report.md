# Analysis Report

## Bindings Analysis

### Unmanaged 

*   TAR - types Android registered

    *   N = $NTAR$

    *   [./$FILENAME$.TAR.csv](./$FILENAME$.TAR.csv)
    
    *   check: N = $NTAR$ = `TARIG.Count()` + `TARNIG.Count()` = $NTARIG$ + $NTARNIG$ = $SUM_TAR$

*   TARIG - types Android registered found in Google's Mappings

    *   N = $NTARIG$

    *   [./$FILENAME$.TARIG.csv](./$FILENAME$.TARIG.csv)

*   TARNIG - types Android registered NOT found in Google's Mappings

    *   N = $NTARNIG$

    *   [./$FILENAME$.TARNIG.csv](./$FILENAME$.TARNIG.csv)

*   TARNIGF - types Android registered NOT found in Google's Mappings FIXED 

    *   FIXED - containing type found in Google's Mappings

    *   N = $NTARNIGF$

    *   [./$FILENAME$.TARNIGF.csv](./$FILENAME$.TARNIGF.csv)

*   TNAR - types nested Android registered

    *   N = $NTNAR$

    *   [./$FILENAME$.TNAR.csv](./$FILENAME$.TNAR.csv)
    
    *   check: N = $NTNAR$ = `TNARIG.Count()` + `TNARNIG.Count()` = $NTNARIG$ + $NTNARNIG$ = $SUM_TNAR$

*   TNARIG - types nested Android registered in Google\'s mappings

    *   N = $NTNARIG$

    *   [./$FILENAME$.TNARIG.csv](./$FILENAME$.TNARIG.csv)
    
*   TNARNIG - types nested Android registered NOT in Google's mappings

    *   N = $NTNARNIG$

    *   [./$FILENAME$.TNARNIG.csv](./$FILENAME$.TNARNIG.csv)
    
*   TNARNIGF - types nested Android registered NOT in Google's mappings FIXED

    *   FIXED - containing type found in Google's Mappings
    
    *   N = $NTNARNIGF$

    *   [./$FILENAME$.TNARNIGF.csv](./$FILENAME$.TNARNIGF.csv)
    
*   TAUR - types Android unregistered 
        
    *   N = $NTAUR$

    *   [./$FILENAME$.TAUR.csv](./$FILENAME$.TAUR.csv)
    
*   TR - type references

    *   N = $NTR$

    *   [./$FILENAME$.TR.csv](./$FILENAME$.TR.csv)
    
## Managed 

*   MappingsForMigrationMergeJoin

    *   N = $N_MappingsForMigrationMergeJoin$

    *   [./$FILENAME$.MappingsForMigrationMergeJoin.csv](./$FILENAME$.MappingsForMigrationMergeJoin.csv)
        
    
## Google Mappings

GoogleMapping = $GoogleMappings$;

### Size Reduction

$SizeReductionReport$


## Artifacts for downloads

AndroidX: 

*   https://dev.azure.com/xamarin/public/_build/results?buildId=517

*   https://github.com/xamarin/AndroidSupportComponents/commit/35c1e15052f0f2f2ef6088ced006e959508d97b7

Android Support: 

*   https://dev.azure.com/xamarin/public/_build/results?buildId=578&view=results

*   https://github.com/xamarin/AndroidSupportComponents/commit/df95df2721031fc533e29e39059517757010d432


