#valgrind analysis
mkdir out/valgrind
for f in out/crashes/id*
do
  export filename="$(basename $f)"
  timeout 1m valgrind --log-file=out/valgrind/crash_log_"$filename"  "..${1}" "$f"
done