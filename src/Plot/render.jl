using Pkg
Pkg.add("DataFrames")
Pkg.add("Plots")
Pkg.add("CSV")

import DataFrames, Plots, CSV
using Plots;

ENV["GKSwstype"]="nul"

function render(csv_df, filePrefix, edgeWeight)
    csv_names = deleteat!(names(csv_df), 1);

    suffix = if edgeWeight == ""
        ""
    else
        " ($(edgeWeight))"
    end

    plot(
        csv_df.Nodes,
        [ csv_df[:, name] for name in csv_names ],
        seriestype=:scatter,
        label=reshape(["$(n)" for n in csv_names], (1, :)),
        xlabel = "Nodes",
        xaxis=:log,
        ylabel = "Time in Seconds",
        yaxis=:log,
        title = "Solving Time$(suffix)",
        legend=:topleft,
        minorgrid=true
    )
    savefig("metrics/plots/$(filePrefix)-plot-log.png")

    plot(
        csv_df.Nodes,
        [ csv_df[:, name] for name in csv_names ],
        seriestype=:scatter,
        label=reshape(["$(n)" for n in csv_names], (1, :)),
        xlabel = "Nodes",
        ylabel = "Time in Seconds",
        title = "Solving Time$(suffix)",
        legend=:topleft,
        minorgrid=true
    )
    savefig("metrics/plots/$(filePrefix)-plot-linear.png")

    plot(
        csv_df.Nodes,
        [
            [
                (csv_df[:, name][i]) / (csv_df.Nodes[i] ^ 2)
                for i in 1:length(csv_df[:, name])
            ] for name in csv_names
        ],
        seriestype=:scatter,
        label=reshape(["$(n)" for n in csv_names], (1, :)),
        xlabel = "Nodes",
        xaxis=:log,
        ylabel = "Time in Seconds / Nodes²",
        title = "Solving Time (normalized)$(suffix)",
        legend=:topleft,
        minorgrid=true
    )
    savefig("metrics/plots/$(filePrefix)-plot-normalized.png")

end

csv_df = CSV.read("metrics/evaluation/node-count-timing.csv", DataFrames.DataFrame)
render(csv_df, "full", "")

edge_names = CSV.read("metrics/evaluation/edge-weight-types.csv", DataFrames.DataFrame)

for name in edge_names.Type
    local csv_df = CSV.read("metrics/evaluation/node-count-timing-$(name).csv", DataFrames.DataFrame)
    render(csv_df, name, name)
end
