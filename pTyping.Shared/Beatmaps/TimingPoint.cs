using Realms;

namespace pTyping.Shared.Beatmaps;

public class TimingPoint : RealmObject {
    public double Time  { get; set; }
    public double Tempo { get; set; }

    public TimingPoint() {}

    public TimingPoint(double time, double tempo) {
        this.Time  = time;
        this.Tempo = tempo;
    }
}
