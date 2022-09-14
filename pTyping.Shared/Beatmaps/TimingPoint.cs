using Realms;

namespace pTyping.Shared.Beatmaps;

public class TimingPoint : RealmObject {
    public double Time;
    public double Tempo;

    public TimingPoint(double time, double tempo) {
        this.Time  = time;
        this.Tempo = tempo;
    }
}
