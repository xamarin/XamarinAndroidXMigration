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

*   TNAR - types nested Android registered

    *   N = $NTNAR$

    *   [./$FILENAME$.TNAR.csv](./$FILENAME$.TNAR.csv)
    
    *   check: N = $NTNAR$ = `TNARIG.Count()` + `TNARNIG.Count()` = $NTNARIG$ + $NTNARNIG$ = $SUM_TNAR$

*   TNARIG - types nested Android registered in Google\'s mappings

    *   N = $NTNARIG$

    *   [./$FILENAME$.TNARIG.csv](./$FILENAME$.TNARIG.csv)
    
*   TNARNIG - types nested Android registered NOT in Google\'s mappings

    *   N = $NTNARNIG$

    *   [./$FILENAME$.TNARNIG.csv](./$FILENAME$.TNARNIG.csv)
    
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


