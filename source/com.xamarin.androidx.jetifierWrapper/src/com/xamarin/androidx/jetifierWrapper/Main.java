package com.xamarin.androidx.jetifierWrapper;

import com.android.tools.build.jetifier.core.config.*;
import com.android.tools.build.jetifier.core.utils.Log;
import com.android.tools.build.jetifier.processor.FileMapping;
import com.android.tools.build.jetifier.processor.Processor;
import com.android.tools.build.jetifier.processor.archive.*;

import org.apache.commons.cli.*;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.file.*;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.concurrent.*;

import static com.android.tools.build.jetifier.standalone.Main.Companion;

public class Main {
    private static final int MAX_THREADS = 10;
    private static final Options OPTIONS;
    private static Option INPUT_OPTION;
    private static Option OUTPUT_OPTION;
    private static Option CONFIG_OPTION;
    private static Option LOG_LEVEL_OPTION;
    private static Option REVERSED_OPTION;
    private static Option STRICT_OPTION;
    private static Option REBUILD_TOP_OF_TREE_OPTION;
    private static Option STRIP_SIGNATURES_OPTION;
    private static Option PARALLEL_OPTION;
    private static Option NO_PARALLEL_OPTION;
    private static Option HELP_OPTION;

    private static boolean IS_REVERSED;
    private static boolean IS_STRICT;
    private static boolean SHOULD_REBUILD_TOP_OF_TREE;
    private static boolean SHOULD_STRIP_SIGNATURES;

    private static CommandLine COMMAND_LINE;

    private static String[] INPUT_VALUES;
    private static String[] OUTPUT_VALUES;

    private static Config JETIFIER_CONFIG;
    private static Processor JETIFIER_PROCESSOR;

    private static long START_TIME;
    private static long END_TIME;

    public static void main(String[] args) {
        createOptionsFromJetifier();
        assignOptions();

        COMMAND_LINE = parseArguments(args);

        if (COMMAND_LINE == null) {
            System.exit(1);
            return;
        }

        if (COMMAND_LINE.hasOption(HELP_OPTION.getOpt())) {
            printHelp();
            System.exit(0);
            return;
        }

        if (COMMAND_LINE.hasOption(PARALLEL_OPTION.getOpt()) && COMMAND_LINE.hasOption(NO_PARALLEL_OPTION.getOpt())) {
            Log.INSTANCE.e("Main", "The option noParallel cannot be used with p|parallel.");
            printHelp();
            System.exit(1);
        }

        INPUT_VALUES = COMMAND_LINE.getOptionValues(INPUT_OPTION.getOpt());
        OUTPUT_VALUES = COMMAND_LINE.getOptionValues(OUTPUT_OPTION.getOpt());

        int inputsLength = INPUT_VALUES.length;
        int outputsLength = OUTPUT_VALUES.length;

        if (inputsLength != outputsLength) {
            Log.INSTANCE.e("Main", "The length of inputs and outputs must be the same.");
            printHelp();
            System.exit(1);
            return;
        }

        String[] arguments = getArgumentsForJetifier();
        JETIFIER_CONFIG = ConfigParser.INSTANCE.loadDefaultConfig();
        JETIFIER_PROCESSOR = Processor.Companion.createProcessor3(JETIFIER_CONFIG, IS_REVERSED, SHOULD_REBUILD_TOP_OF_TREE, !IS_STRICT, false, SHOULD_STRIP_SIGNATURES, null);


        try {
            if (COMMAND_LINE.hasOption(NO_PARALLEL_OPTION.getOpt()))
                executeJetifier(arguments);
            else
                executeJetifierInParallel(arguments);
        } catch (IOException e) {
            e.printStackTrace();
            System.exit(1);
        }

        END_TIME = System.currentTimeMillis();
        double duration = (END_TIME - START_TIME) / 1000.0;

        Log.INSTANCE.i("Main","The jetifier process took: " + duration + " seconds");
    }

    private static void createOptionsFromJetifier() {
        for (Option option : Companion.getOPTIONS().getOptions()) {
            String description = option.getDescription();
            boolean hasArguments = option.hasArgs();

            switch (option.getOpt()) {
                case "i":
                case "o":
                    description += ". Can be used multiple times";
                    hasArguments = true;
                    break;
                case "c":
                case "l":
                    hasArguments = true;
                default:
                    break;
            }

            OPTIONS.addOption(createOption(option.getOpt(), option.getLongOpt(), description, hasArguments, option.isRequired()));
        }

        OPTIONS.addOption(createOption("p", "parallel", "Jetifiy the aar/zip files in parallel. This is by default. Cannot be used with noParallel", false, false));
        OPTIONS.addOption(createOption("noParallel", "noParallel", "Jetifiy the aar/zip files in sequential. Cannot be used with p|parallel", false, false));
        OPTIONS.addOption(createOption("h", "help", "Show this message", false, false));
    }

    private static final Option createOption(final String argumentName, final String argumentNameLong, final String description, final boolean hasArguments, final boolean isRequired) {
        final Option op = new Option(argumentName, argumentNameLong, hasArguments, description);
        op.setRequired(isRequired);
        return op;
    }

    private static void assignOptions() {
        INPUT_OPTION = OPTIONS.getOption("i");
        OUTPUT_OPTION = OPTIONS.getOption("o");
        CONFIG_OPTION = OPTIONS.getOption("c");
        LOG_LEVEL_OPTION = OPTIONS.getOption("l");
        REVERSED_OPTION = OPTIONS.getOption("r");
        STRICT_OPTION = OPTIONS.getOption("s");
        REBUILD_TOP_OF_TREE_OPTION = OPTIONS.getOption("rebuildTopOfTree");
        STRIP_SIGNATURES_OPTION = OPTIONS.getOption("stripSignatures");
        PARALLEL_OPTION = OPTIONS.getOption("p");
        NO_PARALLEL_OPTION = OPTIONS.getOption("noParallel");
        HELP_OPTION = OPTIONS.getOption("h");
    }

    private static CommandLine parseArguments(String[] args) {
        try {
            return new DefaultParser().parse(OPTIONS, args);
        } catch (ParseException e) {
            Log.INSTANCE.e("Main", e.getMessage());
            printHelp();
            return null;
        }
    }

    private static void printHelp() {
        new HelpFormatter().printHelp("Jetifier Wrapper", OPTIONS);
    }

    private static void executeJetifier(String[] arguments) throws IOException {
        Log.INSTANCE.i("Main", "Executing jetifier tool in sequential way.");

        int argumentsLength = arguments.length;
        int inputsLength = INPUT_VALUES.length;

        for (int i = 0, j = argumentsLength - 3; i < inputsLength; i++) {
            String inputFile = INPUT_VALUES[i];
            String outputFile = OUTPUT_VALUES[i];

            Log.INSTANCE.v("Main", "Jetifiying " + inputFile + " file into " + outputFile + " file.");

            if (inputFile.toLowerCase().endsWith(".aar") || inputFile.toLowerCase().endsWith(".zip") || inputFile.toLowerCase().endsWith(".jar")) {
                String[] iterationArguments = Arrays.copyOf(arguments, argumentsLength);
                iterationArguments[j] = inputFile;
                iterationArguments[j + 2] = outputFile;

                Companion.main(iterationArguments);
            } else {
                ArchiveFile archiveFile = getArchiveFile(inputFile);
                JETIFIER_PROCESSOR.visit(archiveFile);
                save(archiveFile, outputFile);
            }

            Log.INSTANCE.v("Main", "File " + inputFile + " jetified into " + outputFile + " file.");
        }
    }

    private static void executeJetifierInParallel(String[] arguments) throws IOException {
        Log.INSTANCE.i("Main", "Executing jetifier tool in parallel way.");

        int argumentsLength = arguments.length;
        int inputsLength = INPUT_VALUES.length;

        int numberOfThreads = inputsLength > MAX_THREADS ? MAX_THREADS : inputsLength;
        ExecutorService executor = Executors.newFixedThreadPool(numberOfThreads);
        List<Future<JetifierData>> jetifierWorks = new ArrayList<>();

        for (int i = 0, j = argumentsLength - 3; i < inputsLength; i++) {
            String inputFile = INPUT_VALUES[i];
            String outputFile = OUTPUT_VALUES[i];

            FileMapping fileMapping = new FileMapping(new File (inputFile), new File (outputFile));
            String[] iterationArguments = null;
            ArchiveItem archiveItem = null;

            if (inputFile.toLowerCase().endsWith(".aar") || inputFile.toLowerCase().endsWith(".zip") || inputFile.toLowerCase().endsWith(".jar")) {
                iterationArguments = Arrays.copyOf(arguments, argumentsLength);
                iterationArguments[j] = inputFile;
                iterationArguments[j + 2] = outputFile;
            } else {
                archiveItem = getArchiveFile(inputFile);
            }

            JetifierData jetifierData = new JetifierData(fileMapping, iterationArguments, archiveItem, JETIFIER_PROCESSOR);

            Log.INSTANCE.v("Main", "Jetifiying " + inputFile + " file into " + outputFile + " file.");

            Callable<JetifierData> jetifier = new JetifierCallable(jetifierData);
            Future<JetifierData> jetifierWork = executor.submit(jetifier);
            jetifierWorks.add(jetifierWork);
        }

        for (Future<JetifierData> jetifierWork : jetifierWorks) {
            try {
                JetifierData jetifierData = jetifierWork.get();
                Log.INSTANCE.v("Main", "File " + jetifierData.getFileMapping().getFrom().getAbsolutePath() + " jetified into " + jetifierData.getFileMapping().getTo().getAbsolutePath() + " file.");
            } catch (InterruptedException e) {
                Log.INSTANCE.e("Main", e.getMessage());
            } catch (ExecutionException e) {
                Log.INSTANCE.e("Main", e.getMessage());
            }
        }

        executor.shutdown();
    }

    private static String[] getArgumentsForJetifier() {
        ArrayList<String> argumentsList = new ArrayList<>();

        if (COMMAND_LINE.hasOption(CONFIG_OPTION.getOpt())) {
            argumentsList.add("-" + CONFIG_OPTION.getOpt());
            argumentsList.add(COMMAND_LINE.getOptionValue(CONFIG_OPTION.getOpt()));
        }

        if (COMMAND_LINE.hasOption(LOG_LEVEL_OPTION.getOpt())) {
            String logLevel = COMMAND_LINE.getOptionValue(LOG_LEVEL_OPTION.getOpt());
            Log.INSTANCE.setLevel(logLevel);
            argumentsList.add("-" + LOG_LEVEL_OPTION.getOpt());
            argumentsList.add(logLevel);
        }

        IS_REVERSED = COMMAND_LINE.hasOption(REVERSED_OPTION.getOpt());
        if (IS_REVERSED)
            argumentsList.add("-" + REVERSED_OPTION.getOpt());

        IS_STRICT = COMMAND_LINE.hasOption(STRICT_OPTION.getOpt());
        if (IS_STRICT)
            argumentsList.add("-" + STRICT_OPTION.getOpt());

        SHOULD_REBUILD_TOP_OF_TREE = COMMAND_LINE.hasOption(REBUILD_TOP_OF_TREE_OPTION.getOpt());
        if (SHOULD_REBUILD_TOP_OF_TREE)
            argumentsList.add("-" + REBUILD_TOP_OF_TREE_OPTION.getOpt());

        SHOULD_STRIP_SIGNATURES = COMMAND_LINE.hasOption(STRIP_SIGNATURES_OPTION.getOpt());
        if (SHOULD_STRIP_SIGNATURES)
            argumentsList.add("-" + STRIP_SIGNATURES_OPTION.getOpt());

        argumentsList.add("-i");
        argumentsList.add("");
        argumentsList.add("-o");
        argumentsList.add("");

        String[] arguments = new String[argumentsList.size()];
        return argumentsList.toArray(arguments);
    }

    public static ArchiveFile getArchiveFile(String inputFilename) throws IOException {
        Path path = Paths.get(inputFilename);
        byte[] fileContent = Files.readAllBytes(path);
        return new ArchiveFile(path, fileContent);
    }

    private static void save(ArchiveItem archiveItem, String outputFilename) throws IOException {
        FileOutputStream outputStream = new FileOutputStream(outputFilename);
        archiveItem.writeSelfTo(outputStream);
    }

    static {
        START_TIME = System.currentTimeMillis();
        OPTIONS = new Options();
    }

}

class JetifierCallable implements Callable<JetifierData> {
    private JetifierData jetifierData;

    public JetifierCallable (JetifierData jetifierData) {
        this.jetifierData = jetifierData;
    }

    @Override
    public JetifierData call() throws Exception {
        if (jetifierData.getArguments() != null)
            Companion.main(jetifierData.getArguments());
        else {
            jetifierData.getProcessor().visit((ArchiveFile)jetifierData.getArchiveItem());
            save(jetifierData.getArchiveItem(), jetifierData.getFileMapping().getTo().getAbsolutePath());
        }

        return jetifierData;
    }

    private void save(ArchiveItem archiveItem, String outputFilename) throws IOException {
        FileOutputStream outputStream = new FileOutputStream(outputFilename);
        archiveItem.writeSelfTo(outputStream);
    }
}

class JetifierData {
    private FileMapping fileMapping;
    private String[] arguments;
    private ArchiveItem archiveItem;
    private Processor processor;

    public JetifierData (FileMapping fileMapping, String[] arguments, ArchiveItem archiveItem, Processor processor) {
        this.fileMapping = fileMapping;
        this.arguments = arguments;
        this.archiveItem = archiveItem;
        this.processor = processor;
    }

    public String[] getArguments() {
        return arguments;
    }

    public FileMapping getFileMapping() {
        return fileMapping;
    }

    public ArchiveItem getArchiveItem() {
        return archiveItem;
    }

    public Processor getProcessor() {
        return processor;
    }
}