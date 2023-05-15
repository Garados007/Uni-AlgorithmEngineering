using Pkg
Pkg.add("DataFrames")
Pkg.add("Plots")
Pkg.add("CSV")

import DataFrames, Plots, CSV
using Plots;

ENV["GKSwstype"]="nul"

csv_df = CSV.read("metrics/evaluation/node-count-timing.csv", DataFrames.DataFrame)

csv_names = deleteat!(names(csv_df), 1);

plot(
    csv_df.Nodes,
    [ csv_df[:, name] for name in csv_names ],
    seriestype=:scatter,
    label=reshape(["$(n)" for n in csv_names], (1, :)),
    xlabel = "Nodes",
    xaxis=:log,
    ylabel = "Time in Seconds",
    yaxis=:log,
    title = "Solving Time",
    legend=:topleft,
    minorgrid=true
)
savefig("metrics/plots/full-plot-log.png")

plot(
    csv_df.Nodes,
    [ csv_df[:, name] for name in csv_names ],
    seriestype=:scatter,
    label=reshape(["$(n)" for n in csv_names], (1, :)),
    xlabel = "Nodes",
    ylabel = "Time in Seconds",
    title = "Solving Time",
    legend=:topleft,
    minorgrid=true
)
savefig("metrics/plots/full-plot-linear.png")
